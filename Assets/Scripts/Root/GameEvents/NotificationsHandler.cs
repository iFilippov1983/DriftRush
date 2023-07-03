using RaceManager.Cars;
using RaceManager.UI;
using RaceManager.Progress;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UniRx;

namespace RaceManager.Root
{
    public class NotificationsHandler
    {
        private Profiler _profiler;
        private CarsDepot _playerCarsDepot;
        private CarUpgradesHandler _upgradesHandler;
        private SpritesContainerCarCollection _carSprites;
        private NotificationPopup _popup;

        [ShowInInspector]
        private Dictionary<CarName, CarInfo> _carsInfo;

        public Action<CarName> OnNotification;

        public NotificationsHandler(Profiler profiler, CarsDepot carsDepot, CarUpgradesHandler upgradesHandler, SpritesContainerCarCollection carSprites)
        {
            _profiler = profiler;
            _playerCarsDepot = carsDepot;
            _upgradesHandler = upgradesHandler;
            _carSprites = carSprites;
        }

        public void Initialize(NotificationPopup popup)
        {
            _popup = popup;
            _carsInfo = new Dictionary<CarName, CarInfo>();

            foreach (CarProfile profile in _playerCarsDepot.ProfilesList)
            {
                if (!_carsInfo.ContainsKey(profile.CarName))
                {
                    var scheme = profile.RankingScheme;

                    CarName name = profile.CarName;

                    CarInfo info = new CarInfo()
                    {
                        needsNotification = false,
                        isAvailable = scheme.CarIsAvailable,
                        isUpgradeable = scheme.GetNextRank().IsReached && !scheme.GetNextRank().IsGranted,
                        isNotified = false,
                        cashedCardsAmount = _profiler.GetCardsAmount(name),
                    };

                    _carsInfo.Add(name, info);
                }
            }

            _upgradesHandler.OnCarUpdate
                .Where(d => d.gotRankUpdate == true)
                .Subscribe(d => UpdateCarInfo(d));
        }

        public void NotifyIfNeeded()
        {
            foreach (var info in _carsInfo)
            {
                if (info.Value.needsNotification)
                {
                    CarName carName = info.Key;

                    CarInfo carInfo = info.Value;

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

                    _carsInfo[carName] = carInfo;

                    return;
                }
            }
        }

        private void UpdateCarInfo(CarUpdateData data)
        {
            CarInfo carInfo = _carsInfo[data.carName];
            CarRankingScheme scheme = _playerCarsDepot.GetProfile(data.carName).RankingScheme;

            int cardsAmount = _profiler.GetCardsAmount(data.carName);
            int cardsAmountToUpgrade = scheme.GetNextRank().PointsForAccess;

            carInfo.needsNotification = data.gotUnlocked || cardsAmount > cardsAmountToUpgrade;
            carInfo.isAvailable = carInfo.isAvailable != data.gotUnlocked;
            carInfo.isUpgradeable = cardsAmount > cardsAmountToUpgrade;
            carInfo.isNotified = !carInfo.needsNotification;
            carInfo.cashedCardsAmount = cardsAmount;

            _carsInfo[data.carName] = carInfo;

            //$"Updating car info {data.carName} => Current rank {scheme.CurrentRank.Rank} => Cur cards amount: {cardsAmount} => Goal cards amount: {cardsAmountToUpgrade}".Log(Logger.ColorYellow);
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
