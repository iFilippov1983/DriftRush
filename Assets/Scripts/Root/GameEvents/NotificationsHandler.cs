using RaceManager.Cars;
using RaceManager.UI;
using RaceManager.Progress;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UniRx;
using UniRx.Triggers;

namespace RaceManager.Root
{
    public class NotificationsHandler : IDisposable
    {
        private Profiler _profiler;
        private CarsDepot _playerCarsDepot;
        private SpritesContainerCarCollection _carSprites;
        private NotificationPopup _popup;

        [ShowInInspector]
        private Dictionary<CarName, CarInfo> _cars;

        public Action<CarName> OnNotification;

        public NotificationsHandler(Profiler profiler, CarsDepot carsDepot, SpritesContainerCarCollection carSprites)
        {
            _profiler = profiler;
            _playerCarsDepot = carsDepot;
            _carSprites = carSprites;
        }

        public void Initialize(NotificationPopup popup)
        {
            _popup = popup;
            _cars = new Dictionary<CarName, CarInfo>();

            foreach (CarProfile profile in _playerCarsDepot.ProfilesList)
            {
                if (!_cars.ContainsKey(profile.CarName))
                {
                    var scheme = profile.RankingScheme;

                    CarName name = profile.CarName;

                    CarInfo info = new CarInfo()
                    {
                        needsNotification = false,
                        isAvailable = scheme.CarIsAvailable,
                        isUpgradeable = scheme.CurrentRank.IsReached && !scheme.CurrentRank.IsGranted,
                        isNotified = false,
                        cashedCardsAmount = _profiler.GetCardsAmount(name),
                    };

                    _cars.Add(name, info);
                }
            }

            _profiler.OnCarCardsAmountChange += UpdateCarInfo;
        }

        public void NotifyIfNeeded()
        {
            foreach (var car in _cars)
            {
                if (car.Value.needsNotification)
                {
                    CarName carName = car.Key;

                    CarInfo carInfo = car.Value;

                    string name = carName.ToString().SplitByUppercaseWith(" ");
                    name = name.Replace('_', ' ');
                    _popup.CarNameText.text = name.ToUpper();

                    Sprite carSprite = _carSprites.GetCarSprite(carName);
                    _popup.CarImage.sprite = carSprite;

                    if (carInfo.isUpgradeable)
                    {
                        _popup.UpgradeCarButton.SetActive(true);
                        _popup.CanUpgradeCarText.SetActive(true);

                        _popup.OpenCollectionButton.SetActive(false);
                        _popup.UnlockedCarText.SetActive(false);
                    }
                    else if (carInfo.isAvailable)
                    {
                        _popup.UpgradeCarButton.SetActive(false);
                        _popup.CanUpgradeCarText.SetActive(false);

                        _popup.OpenCollectionButton.SetActive(true);
                        _popup.UnlockedCarText.SetActive(true);
                    }

                    carInfo.needsNotification = false;
                    carInfo.isNotified = true;

                    OnNotification?.Invoke(carName);

                    _cars[carName] = carInfo;

                    return;
                }
            }
        }

        private void UpdateCarInfo(CarName carName, int cardsAmount)
        {
            CarInfo carInfo = _cars[carName];

            if (carInfo.cashedCardsAmount > cardsAmount)
            { 
                carInfo.cashedCardsAmount = cardsAmount;
                _cars[carName] = carInfo;
                return;
            }

            CarRankingScheme scheme = _playerCarsDepot.GetProfile(carName).RankingScheme;

            carInfo.needsNotification =
                carInfo.isAvailable != scheme.CarIsAvailable
                || carInfo.isUpgradeable != scheme.CurrentRank.IsReached && !scheme.CurrentRank.IsGranted;

            carInfo.isAvailable = scheme.CarIsAvailable;
            carInfo.isUpgradeable = scheme.CurrentRank.IsReached && !scheme.CurrentRank.IsGranted;
            carInfo.isNotified = !carInfo.needsNotification;
            carInfo.cashedCardsAmount = cardsAmount;

            _cars[carName] = carInfo;

            $"Updating car info {carName} => Current rank {scheme.CurrentRank.Rank}".Log();
        }

        public void Dispose()
        {
            _profiler.OnCarCardsAmountChange -= UpdateCarInfo;
        }

        private struct CarInfo
        {
            public bool needsNotification;
            public bool isAvailable;
            public bool isUpgradeable;
            public bool isNotified;
            public int cashedCardsAmount;
        }
    }
}
