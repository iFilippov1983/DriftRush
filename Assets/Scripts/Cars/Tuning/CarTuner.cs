using RaceManager.Root;
using System;
using UniRx;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarTuner : IDisposable
    {
        private const int Threshold_0 = 25;
        private const int Threshold_1 = 50;
        private const int Threshold_2 = 75;
        private const int Threshold_3 = 100;

        private CarsDepot _playerCarDepot;
        private CarProfile _carProfile;
        //private CarConfigVisual _carConfigVisual;
        private CarVisual _carVisual;


        public Action OnCurrentCarChanged;
        public Action<CarCharacteristicsType, float> OnCharacteristicValueChanged;

        public CarTuner(CarsDepot playerCarDepot)
        {
            _playerCarDepot = playerCarDepot;
            _carProfile = _playerCarDepot.CurrentCarProfile;
            //_carConfigVisual = _carProfile.CarConfigVisual;



            OnCurrentCarChanged += ChangeCar;
            OnCharacteristicValueChanged += ChangeVisuals;
        }

        public void SetCarVisualToTune(CarVisual carVisual)
        {
            _carVisual = carVisual;
        }

        private void ChangeVisuals(CarCharacteristicsType characteristics, float value)
        {
            switch (characteristics)
            {
                case CarCharacteristicsType.Speed:
                    TuneSpeed(value);
                    break;
                case CarCharacteristicsType.Mobility:
                    TuneMobility(value);
                    break;
                case CarCharacteristicsType.Durability:
                    TuneDurability(value);
                    break;
                case CarCharacteristicsType.Acceleration:
                    break;
            }
        }

        private void TuneSpeed(float value)
        {
            int newValue = Mathf.RoundToInt(value);
            _carProfile.CarCharacteristics.CurrentSpeedFactor = newValue;
            PartLevel level = GetPartLevel(CarCharacteristicsType.Speed);

            var carConfigVisual = _carProfile.CarConfigVisual;
            carConfigVisual.CurrentWheelsLevel = level;
            _carVisual.SetPartsVisual(PartType.Wheel, carConfigVisual.CurrentWheelsLevel, carConfigVisual.CurrentWheelsSetType);
            //_carConfigVisual.CurrentWheelsLevel = level;
            //_carVisual.SetPartsVisual(PartType.Wheel, _carConfigVisual.CurrentWheelsLevel, _carConfigVisual.CurrentWheelsSetType);

            _playerCarDepot.UpdateProfile(_carProfile);
        }

        private void TuneMobility(float value)
        { 
            int newValue = Mathf.RoundToInt(value);
            _carProfile.CarCharacteristics.CurrentMobilityFactor = newValue;
            PartLevel level = GetPartLevel(CarCharacteristicsType.Mobility);

            var carConfigVisual = _carProfile.CarConfigVisual;
            carConfigVisual.CurrentSuspentionLevel = level;
            _carVisual.SetPartsVisual(PartType.Suspention, carConfigVisual.CurrentSuspentionLevel);

            _playerCarDepot.UpdateProfile(_carProfile);
        }

        private void TuneDurability(float value)
        {
            int newValue = Mathf.RoundToInt(value);
            _carProfile.CarCharacteristics.CurrentDurabilityFactor = newValue;
            PartLevel level = GetPartLevel(CarCharacteristicsType.Durability);

            var carConfigVisual = _carProfile.CarConfigVisual;
            carConfigVisual.CurrentSuspentionLevel = level;
            _carVisual.SetPartsVisual(PartType.Bumper, carConfigVisual.CurrentBumpersLevel);

            _playerCarDepot.UpdateProfile(_carProfile);
        }

        private void TuneAcceleration(float value)
        {
            int newValue = Mathf.RoundToInt(value);
            _carProfile.CarCharacteristics.CurrentMobilityFactor = newValue;
            PartLevel level = GetPartLevel(CarCharacteristicsType.Mobility);

            var carConfigVisual = _carProfile.CarConfigVisual;
            carConfigVisual.CurrentSuspentionLevel = level;
            _carVisual.SetPartsVisual(PartType.Suspention, carConfigVisual.CurrentSuspentionLevel);

            _playerCarDepot.UpdateProfile(_carProfile);
        }

        private PartLevel GetPartLevel(CarCharacteristicsType characteristics)
        {
            var c = _carProfile.CarCharacteristics;
            int percentage = characteristics switch
            {
                CarCharacteristicsType.Speed => Mathf.RoundToInt(c.CurrentSpeedFactor * 100 / c.MaxSpeedFactor),
                CarCharacteristicsType.Mobility => Mathf.RoundToInt(c.CurrentMobilityFactor * 100 / c.MaxMobilityFactor),
                CarCharacteristicsType.Durability => Mathf.RoundToInt(c.CurrentDurabilityFactor * 100 / c.MaxDurabilityFactor),
                CarCharacteristicsType.Acceleration => Mathf.RoundToInt(c.CurrentAccelerationFactor * 100 / c.MaxAccelerationFactor),
                _ => throw new NotImplementedException(),
            };

            PartLevel pLevel = PartLevel.Zero;
            if (percentage <= Threshold_0)
                pLevel = PartLevel.Zero;
            else if (Threshold_0 < percentage && percentage <= Threshold_1)
                pLevel = PartLevel.First;
            else if (Threshold_1 < percentage && percentage <= Threshold_2)
                pLevel = PartLevel.Second;
            else if (Threshold_2 < percentage && percentage <= Threshold_3)
                pLevel = PartLevel.Third;

            Debug.Log($"CHAR: {characteristics} => PERCENTAGE: {percentage} => LEVEL: {pLevel}");

            return pLevel;
        }

        private void ChangeCar()
        {
            _carProfile = _playerCarDepot.CurrentCarProfile;
            //_carConfigVisual = _carProfile.CarConfigVisual;
        }

        public void Dispose()
        {
            OnCurrentCarChanged -= ChangeCar;
            OnCharacteristicValueChanged -= ChangeVisuals;
        }



    }
}