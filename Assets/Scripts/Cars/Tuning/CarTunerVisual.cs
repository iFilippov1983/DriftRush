using RaceManager.Root;
using System;
using UniRx;

namespace RaceManager.Cars
{
    public class CarTunerVisual : IDisposable
    {
        private CarsDepot _playerCarDepot;
        private CarConfigVisual _carConfigVisual;
        private SaveManager _saveManager;

        public Action OnCurrentCarChanged;
        public Action<float> OnSpeedValueChanged;
        public Action<float> OnMobilityValueChanged;
        public Action<float> OnDurabilityValueChanged;
        public Action<float> OnAccelerationValueChanged;

        public CarTunerVisual(CarsDepot playerCarDepot, SaveManager saveManager)
        {
            _playerCarDepot = playerCarDepot;
            _carConfigVisual = _playerCarDepot.CurrentCarProfile.CarConfigVisual;
            _saveManager = saveManager;

            OnCurrentCarChanged += ChangeVisualContainer;
            OnSpeedValueChanged += OnSpeedChange;
        }

        private void OnSpeedChange(float value)
        {

        }

        private void ChangeVisualContainer() => _carConfigVisual = _playerCarDepot.CurrentCarProfile.CarConfigVisual;

        public void Dispose()
        {
            OnCurrentCarChanged -= ChangeVisualContainer;
        }
    }
}