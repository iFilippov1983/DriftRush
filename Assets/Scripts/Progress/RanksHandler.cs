using RaceManager.Cars;
using RaceManager.Root;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace RaceManager.Progress
{
    public class RanksHandler : IDisposable
    {
        private Profiler _profiler;
        private CarsDepot _carsDepot;
        private SaveManager _saveManager;

        public Action<CarName> OnCarRankUpdate;

        [Inject]
        private void Construct(Profiler profiler, CarsDepot carsDepot, SaveManager saveManager)
        { 
            _profiler = profiler;
            _carsDepot = carsDepot;
            _saveManager = saveManager;

            _profiler.OnCarCardsAdded += UpdateRankProgress;
        }

        private void UpdateRankProgress(CarName carName, int cardsAmount)
        {
            CarProfile profile = _carsDepot.CarProfiles.Find(p => p.CarName == carName);
            var carRank = profile.RankingScheme.CurrentRank;

            if (cardsAmount >= carRank.PointsForAccess)
            {
                if (carRank.Rank == Rank.Rank_1)
                {
                    bool success = _profiler.TryBuyWithCards(carName, carRank.AccessCost);
                    if (success)
                        carRank.IsGranted = true;
                }

                carRank.IsReached = true;
            }

            OnCarRankUpdate?.Invoke(carName);
            _saveManager.Save();
        }


        public void Dispose()
        {
            _profiler.OnCarCardsAdded -= UpdateRankProgress;
        }
    }
}
