using RaceManager.Cars;
using RaceManager.Root;
using RaceManager.UI;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Shed
{
    public class ShedHandler : MonoBehaviour, Root.IInitializable, IDisposable
    {
        [SerializeField] private MaterialsContainer _materialsContainer;
        private CarsDepot _playerCarDepot;
        private MainUI _mainUI;
        private PodiumView _podium;
        private CarTuner _carTuner;
        private CarVisual _carVisual;
        private SaveManager _saveManager;
        private CarDestroyer _carDestroyer;

        [Inject]
        private void Construct(CarsDepot playerCarDepot, CarTuner carTuner, MainUI mainUI, PodiumView podium, SaveManager saveManager)
        {
            _carTuner = carTuner;
            _mainUI = mainUI;
            _podium = podium;
            _playerCarDepot = playerCarDepot;
            _saveManager = saveManager;
        }

        public void Initialize()
        {
            InitializeCar();
            InitializeTunerVisual();
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

        private void InitializeTunerVisual()
        {
            _mainUI.OnCarProfileChange += ChangeCar;
            _carTuner.OnCurrentCarChanged += InitializeNewCar;

            _mainUI.OnSpeedValueChange
                .Subscribe((v) =>
                {
                    //$"Speed - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Speed, v);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });

            _mainUI.OnMobilityValueChange
                .Subscribe((v) =>
                {
                    //$"Mobility - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Mobility, v);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });

            _mainUI.OnDurabilityValueChange
                .Subscribe((v) =>
                {
                    //$"Durability - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Durability, v);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });

            _mainUI.OnAccelerationValueChange
                .Subscribe((v) =>
                {
                    //$"Acceleration - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Acceleration, v);
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

        private void ChangeCar(CarName newCarName)
        {
            _playerCarDepot.CurrentCarName = newCarName;
            _carTuner.ChangeCar();
            _mainUI.UpdateTuningPanelValues();
            _mainUI.UpdateCarsCollectionInfo();
            _saveManager.Save();
            $"SAVE - {this}".Log();
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
