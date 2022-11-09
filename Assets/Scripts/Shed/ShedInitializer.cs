using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Root;
using RaceManager.UI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Shed
{
    public class ShedInitializer : MonoBehaviour, Root.IInitializable
    {
        [SerializeField] private MaterialsContainer _materialsContainer;
        private CarsDepot _playerCarDepot;
        private MainUI _mainUI;
        private Podium _podium;
        private CarTuner _carTuner;
        private CarVisual _carVisual;
        private SaveManager _saveManager;

        [Inject]
        private void Construct(CarsDepot playerCarDepot, CarTuner carTuner, MainUI mainUI, Podium podium, SaveManager saveManager)
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

        //void Start()
        //{
        //    InitializeCar();
        //    InitializeTunerVisual();
        //}

        private void InitializeCar()
        {
            CarFactory carFactory = new CarFactory(_playerCarDepot, _materialsContainer, _podium.CarPlace);
            carFactory.ConstructCarForShed(out _carVisual);
        }

        private void InitializeTunerVisual()
        {
            _carTuner.SetCarVisualToTune(_carVisual);
            _mainUI.OnCarProfileChange += _carTuner.OnCurrentCarChanged;

            _mainUI.OnSpeedValueChange
                .Subscribe((v) =>
                {
                    //$"Speed - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharType.Speed, v);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });

            _mainUI.OnMobilityValueChange
                .Subscribe((v) =>
                {
                    //$"Mobility - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharType.Mobility, v);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });


            _mainUI.OnDurabilityValueChange
                .Subscribe((v) => 
                {
                    //$"Durability - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharType.Durability, v);
                    _mainUI.OnTuneValuesChange?.Invoke(availables);
                    _saveManager.Save();
                });


            _mainUI.OnAccelerationValueChange
                .Subscribe((v) => 
                {
                    //$"Acceleration - On next({v})".Log(ConsoleLog.Color.Yellow);
                    int availables = (int)_carTuner.OnCharacteristicValueChanged?.Invoke(CharType.Acceleration, v);
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
    }
}
