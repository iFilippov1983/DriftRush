using RaceManager.Root;
using System;
using UniRx;

namespace RaceManager.Cars
{
    public class CarTunerVisual : IDisposable
    {
        private CarsDepot _playerCarDepot;
        private CarVisualContainer _visualContainer;
        private SaveManager _saveManager;

        public Action OnCurrentCarChanged;
        public Action<float> OnSpeedValueChanged;
        public Action<float> OnMobilityValueChanged;
        public Action<float> OnDurabilityValueChanged;
        public Action<float> OnAccelerationValueChanged;

        public CarTunerVisual(CarsDepot playerCarDepot, SaveManager saveManager)
        {
            _playerCarDepot = playerCarDepot;
            _visualContainer = _playerCarDepot.CurrentCarProfile.CarVisualContainer;
            _saveManager = saveManager;

            OnCurrentCarChanged += ChangeVisualContainer;
            OnSpeedValueChanged += OnSpeedChange;
        }

        private void OnSpeedChange(float value)
        {

        }

        private void ChangeVisualContainer() => _visualContainer = _playerCarDepot.CurrentCarProfile.CarVisualContainer;

        public void Dispose()
        {
            OnCurrentCarChanged -= ChangeVisualContainer;
        }
    }
}