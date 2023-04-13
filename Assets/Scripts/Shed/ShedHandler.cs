using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.UI;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;
using IInitializable = RaceManager.Root.IInitializable;

namespace RaceManager.Shed
{
    public class ShedHandler : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField] private MaterialsContainer _materialsContainer;
        private CarsDepot _playerCarDepot;
        private MainUI _mainUI;
        private PodiumView _podium;
        private CarTuner _carTuner;
        private CarUpgradesHandler _upgradesHandler;
        private CarVisual _carVisual;
        private SaveManager _saveManager;
        private GameEvents _gameEvents;
        //private CarDestroyer _carDestroyer;

        [Inject]
        private void Construct
            (
            CarsDepot playerCarDepot, 
            CarTuner carTuner, 
            CarUpgradesHandler upgradesHandler,
            MainUI mainUI, 
            PodiumView podium, 
            SaveManager saveManager,
            GameEvents gameEvents
            )
        {
            _playerCarDepot = playerCarDepot;
            _carTuner = carTuner;
            _upgradesHandler = upgradesHandler;
            _mainUI = mainUI;
            _podium = podium;
            _saveManager = saveManager;
            _gameEvents = gameEvents;
        }

        public void Initialize()
        {
            InitializeCar();
            InitializeTuner();
            InitializeHandler();
        }

        private void InitializeCar()
        {
            CarFactory carFactory = new CarFactory(_playerCarDepot, _materialsContainer, _podium.CarPlace);
            carFactory.ConstructCarForShed(out _carVisual);
            _carTuner.SetTuner(_carVisual);
        }

        private void InitializeNewCar()
        {
            //_carDestroyer = new CarDestroyer(_carVisual.gameObject);
            //await Task.WhenAll(_carDestroyer.DestroyCar());
            Destroy(_carVisual.gameObject);

            CarFactory carFactory = new CarFactory(_playerCarDepot, _materialsContainer, _podium.RespawnCarPlace);
            carFactory.ConstructCarForShed(out _carVisual);
            _carTuner.SetTuner(_carVisual);
        }

        private void InitializeTuner()
        {
            _mainUI.OnCarProfileChange += ChangeCar;
            _carTuner.OnCurrentCarChanged += InitializeNewCar;

            _mainUI.OnSpeedValueChange
                .Subscribe((v) =>
                {
                    //$"Speed - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Speed, v, true);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });

            _mainUI.OnHandlingValueChange
                .Subscribe((v) =>
                {
                    //$"Handling - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Handling, v, true);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });

            _mainUI.OnAccelerationValueChange
                .Subscribe((v) =>
                {
                    //$"Acceleration - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Acceleration, v, true);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });

            _mainUI.OnFrictionValueChange
                .Subscribe((v) =>
                {
                    //$"Durability - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Friction, v, true);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });

            _carTuner.OnCharValueLimit
                .AsObservable()
                .Subscribe((td) =>
                {
                    _mainUI.OnCharValueLimit.OnNext(td);
                    _saveManager.Save();
                });
        }

        private void InitializeHandler()
        {
            _upgradesHandler.OnCarUpdate
                .Subscribe(d => 
                {
                    UpdateCarInfo(d.carName, !d.gotRankUpdate);
                });
        }

        private void ChangeCar(CarName newCarName)
        {
            _playerCarDepot.CurrentCarName = newCarName;
            _carTuner.ChangeCar();
            UpdateCarInfo(newCarName);
            _saveManager.Save();
        }

        private void UpdateCarInfo(CarName carName, bool addFactors = false)
        {
            _mainUI.UpdateTuningPanelValues(addFactors);
            _mainUI.UpdateCarsCollectionCard(carName);
            _mainUI.UpdateCarsCollectionInfo();
            _mainUI.UpdateCarWindow();

            _carTuner.SetCarProfile();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                _gameEvents.ScreenTaped.OnNext();

            if (Input.GetMouseButton(0))
                _gameEvents.ScreenTapHold.OnNext();

            if (Input.GetMouseButtonUp(0))
                _gameEvents.ScreenTapReleased.OnNext();
        }

        public void Dispose()
        {
            _mainUI.OnCarProfileChange -= ChangeCar;
            _carTuner.OnCurrentCarChanged -= InitializeNewCar;
        }

        /// <summary>
        /// Use it to visualize car destruction
        /// </summary>
        private class CarDestroyer
        {
            private float _destructionSpeed = 0.075f;
            private GameObject _car;
 
            public CarDestroyer(GameObject carObject)
            {
                _car = carObject;
            }

            public async Task DestroyCar()
            {
                float progress = _car.transform.localScale.y;
                Vector3 scale = _car.transform.localScale;

                while (progress > 0f)
                {
                    scale = Vector3.Lerp(scale, Vector3.zero, _destructionSpeed);
                    _car.transform.localScale = scale;

                    await Task.Yield();
                    progress -= _destructionSpeed;
                }

                Destroy(_car);
            }
        }
    }
}
