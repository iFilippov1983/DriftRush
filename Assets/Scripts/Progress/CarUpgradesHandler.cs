using RaceManager.Cars;
using RaceManager.Root;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Progress
{
    public class CarUpgradesHandler : IDisposable
    {
        private Profiler _profiler;
        private CarsDepot _carsDepot;
        //private CarsUpgradeScheme _upgradeScheme;
        private SaveManager _saveManager;

        public Action<CarName> OnCarRankUpdate;
        public Action<CarName> OnCarFactorsUpgrade;
        public Subject<(CarName name, bool rankUpdate)> OnCarUpdate = new Subject<(CarName name, bool rankUpdate)>();

        //public CarsUpgradeScheme.CarUpgrade CurrentUpgrade
        //{
        //    get
        //    {
        //        CarProfile carProfile = _carsDepot.CurrentCarProfile;
        //        var characteristics = carProfile.CarCharacteristics;
        //        var ranksScheme = carProfile.RankingScheme;

        //        var upgradesList = _upgradeScheme.UpgradesSceme[characteristics.Rarity];
        //        var upgrade = upgradesList?.Find(u => u.NecesseryRank == ranksScheme.CurrentRank.Rank);
        //        return upgrade;
        //    }
        //}

        public CarRankingScheme.CarRank CurrentRank => _carsDepot.CurrentCarProfile.RankingScheme.CurrentRank;

        //public int CurrentUpgradeCost => CurrentUpgrade.Price;
        public int CurrentUpgradeCost => CurrentRank.UpgradeCostCurrent;

        //public float CurrentUpgradeStatsToAdd => CurrentUpgrade.StatsToAdd;
        public float CurrentUpgradeStatsToAdd => CurrentRank.StatsToAddPerUpgrade;

        [Inject]
        //private void Construct(Profiler profiler, CarsDepot playerCarsDepot, CarsUpgradeScheme upgradeScheme, SaveManager saveManager)
        private void Construct(Profiler profiler, CarsDepot playerCarsDepot, SaveManager saveManager)
        { 
            _profiler = profiler;
            _carsDepot = playerCarsDepot;
            //_upgradeScheme = upgradeScheme;
            _saveManager = saveManager;

            InitializeProfiles();
            //_profiler.OnCarCardsAmountChange += UpdateRankProgress;
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

                //OnCarRankUpdate?.Invoke(carProfile.CarName);

                if (scheme.CurrentRank.UpgradeCostCurrent < currentUpgradeCost)
                    scheme.CurrentRank.UpgradeCostCurrent = currentUpgradeCost;

                OnCarUpdate.OnNext((name: carProfile.CarName, rankUpdate: true));

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

                OnCarUpdate.OnNext((name: carProfile.CarName, rankUpdate: false));

                //OnCarFactorsUpgrade?.Invoke(carProfile.CarName);

                _saveManager.Save();

                return true;
            }

            return false;
        }

        //public bool TryUpgradeCurrentCarFactors()
        //{
        //    if (!CanUpgradeCurrentCarFactors())
        //        return false;

        //    CarProfile carProfile = _carsDepot.CurrentCarProfile;
        //    var upgrade = CurrentUpgrade;

        //    if (_profiler.TryBuyWithMoney(upgrade.Price) )
        //    {
        //        carProfile.CarCharacteristics.CurrentFactorsProgress += Mathf.RoundToInt(upgrade.StatsToAdd);
        //        _carsDepot.UpdateProfile(carProfile);

        //        OnCarUpdate.OnNext((name: carProfile.CarName, rankUpdate: false));

        //        //OnCarFactorsUpgrade?.Invoke(carProfile.CarName);

        //        _saveManager.Save();

        //        return true;
        //    }

        //    return false;
        //}

        public bool CanUpgradeCurrentCarFactors()
        {
            var characteristics = _carsDepot.CurrentCarProfile.CarCharacteristics;
            //var upgrade = CurrentUpgrade;
            var rank = CurrentRank;

            int unusedFactors = characteristics.FactorsMaxCurrent - characteristics.CurrentFactorsProgress;

            return rank != null && (unusedFactors / rank.StatsToAddPerUpgrade) >= 1f;
        }

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
                if(TryUnlockCar(profile))
                    return;
            }

            OnCarUpdate.OnNext((name: carName, rankUpdate: true));

            //OnCarRankUpdate?.Invoke(carName);
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

                OnCarUpdate.OnNext((name: carName, rankUpdate: true));

                //OnCarRankUpdate?.Invoke(carName);
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

        public void Dispose()
        {
            //_profiler.OnCarCardsAmountChange -= UpdateRankProgress;
        }
    }
}
