using RaceManager.Cars;
using RaceManager.Root;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Progress
{
    public class CarUpgradesHandler
    {
        private Profiler _profiler;
        private CarsDepot _carsDepot;
        private SaveManager _saveManager;

        public Action<CarName> OnCarRankUpdate;
        public Action<CarName> OnCarFactorsUpgrade;
        public Subject<CarUpdateData> OnCarUpdate = new Subject<CarUpdateData>();

        public CarRankingScheme.CarRank CurrentRank => _carsDepot.CurrentCarProfile.RankingScheme.CurrentRank;
        public int CurrentUpgradeCost => CurrentRank.UpgradeCostCurrent;
        public float CurrentUpgradeStatsToAdd => CurrentRank.StatsToAddPerUpgrade;

        [Inject]
        private void Construct(Profiler profiler, CarsDepot playerCarsDepot, SaveManager saveManager)
        { 
            _profiler = profiler;
            _carsDepot = playerCarsDepot;
            _saveManager = saveManager;

            InitializeProfiles();

            _profiler.OnCarCardsAmountChange
                .AsObservable()
                .Subscribe(t => UpdateRankProgress(t.Item1, t.Item2));
        }

        public bool TryUpgradeCarRank(CarProfile profile = null)
        {
            CarProfile carProfile = profile ?? _carsDepot.CurrentCarProfile;
            var scheme = carProfile.RankingScheme;

            bool canUpgrade = 
                !scheme.CurrentRank.IsGranted 
                && 
                _profiler.TryBuyWithMoney(scheme.CurrentRank.AccessCost)
                &&
                _profiler.TryBuyWithCards(carProfile.CarName, scheme.CurrentRank.PointsForAccess);

            if (canUpgrade)
            {
                int currentUpgradeCost = scheme.CurrentRank.UpgradeCostCurrent;

                scheme.CurrentRank.IsGranted = true;
                UpdateRankProgress(carProfile.CarName, _profiler.GetCardsAmount(carProfile.CarName));
                UpdateCarMaxFactorsCurrent(carProfile);
                _carsDepot.UpdateProfile(carProfile);

                if (scheme.CurrentRank.UpgradeCostCurrent < currentUpgradeCost)
                    scheme.CurrentRank.UpgradeCostCurrent = currentUpgradeCost;

                OnCarUpdate?.OnNext(new CarUpdateData() 
                { 
                    carName = carProfile.CarName,
                    gotRankUpdate = true,
                    gotUnlocked = false
                });

                _saveManager.Save();

                return true;
            }

            return false;
        }

        public bool TryUpgradeCurrentCarFactors()
        {
            if (!CanUpgradeCurrentCarFactors())
                return false;

            CarProfile carProfile = _carsDepot.CurrentCarProfile;
            var rank = CurrentRank;

            if (_profiler.TryBuyWithMoney(rank.UpgradeCostCurrent))
            {
                carProfile.CarCharacteristics.CurrentFactorsProgress += Mathf.RoundToInt(rank.StatsToAddPerUpgrade);
                _carsDepot.UpdateProfile(carProfile);

                rank.UpgradeCostCurrent += Mathf.CeilToInt(rank.UpgradeCostBase * rank.CurrentCostGrowth);

                OnCarUpdate?.OnNext(new CarUpdateData() 
                { 
                    carName = carProfile.CarName,
                    gotRankUpdate = false,
                    gotUnlocked = false
                });

                _saveManager.Save();

                return true;
            }

            return false;
        }

        public bool CanUpgradeCurrentCarFactors()
        {
            var characteristics = _carsDepot.CurrentCarProfile.CarCharacteristics;
            var rank = CurrentRank;

            int unusedFactors = characteristics.FactorsMaxCurrent - characteristics.CurrentFactorsProgress;

            return rank != null && (unusedFactors / rank.StatsToAddPerUpgrade) >= 1f;
        }

        public bool HasMoneyToUpgradeCurrentCarFactors() => _profiler.HasEnoughMoneyFor(CurrentRank.UpgradeCostCurrent);

        public bool HasMoneyToUpgradeCar() => _profiler.HasEnoughMoneyFor(CurrentRank.AccessCost);

        public bool CurrentCarHasMaxFactorsUpgrade()
        {
            var characteristics = _carsDepot.CurrentCarProfile.CarCharacteristics;
            return characteristics.FactorsMaxTotal <= characteristics.CurrentFactorsProgress;
        }

        private void InitializeProfiles()
        {
            foreach (CarProfile carProfile in _carsDepot.ProfilesList)
            {
                if(carProfile.CarCharacteristics.FactorsMaxCurrent == 0)
                    UpdateCarMaxFactorsCurrent(carProfile);
            }
        }

        private void UpdateRankProgress(CarName carName, int cardsAmount = 0)
        {
            CarProfile profile = _carsDepot.GetProfile(carName);
            var carRank = profile.RankingScheme.CurrentRank;

            if (cardsAmount >= carRank.PointsForAccess)
            {
                carRank.IsReached = true;
                if (TryUnlockCar(profile))
                    return;
            }

            OnCarUpdate?.OnNext(new CarUpdateData() 
            { 
                carName = carName,
                gotRankUpdate = true,
                gotUnlocked = false
            });

            _saveManager.Save();
        }

        private bool TryUnlockCar(CarProfile profile)
        {
            var carRank = profile.RankingScheme.CurrentRank;

            if (carRank.IsGranted)
                return false;

            CarName carName = profile.CarName;

            if (carRank.Rank == Rank.Rank_1 && carRank.IsReached)
            {
                carRank.IsGranted = true;
                _profiler.TryBuyWithCards(carName, carRank.PointsForAccess);
                UpdateCarMaxFactorsCurrent(profile);

                OnCarUpdate?.OnNext(new CarUpdateData()
                { 
                    carName = carName,
                    gotRankUpdate = true,
                    gotUnlocked = true
                });

                _saveManager.Save();
                return true;
            }

            return false;
        }

        private void UpdateCarMaxFactorsCurrent(CarProfile carProfile)
        {
            var scheme = carProfile.RankingScheme;

            float availablePecentage = 0;
            foreach (var rank in scheme.Ranks)
            {
                if (rank.IsGranted)
                    availablePecentage = rank.AvailableTunePercentage;
            }

            var characteristics = carProfile.CarCharacteristics;
            int usableFactors = characteristics.FactorsMaxTotal - characteristics.InitialFactorsProgress;
            int availableFactors = Mathf.RoundToInt(usableFactors * availablePecentage);

            characteristics.FactorsMaxCurrent = availableFactors + characteristics.InitialFactorsProgress;
        }
    }
}
