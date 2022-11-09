using System;
using UnityEngine;
using UnityEngine.Events;

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
        private CarVisual _carVisual;

        public Action OnCurrentCarChanged;
        public Func<CharType, float, int> OnCharacteristicValueChanged;
        public UnityEvent<TuneData> OnCharValueLimit = new UnityEvent<TuneData>();

        public CarTuner(CarsDepot playerCarDepot)
        {
            _playerCarDepot = playerCarDepot;
            _carProfile = _playerCarDepot.CurrentCarProfile;

            OnCurrentCarChanged += ChangeCar;
            OnCharacteristicValueChanged += ChangeVisuals;
        }

        public void SetCarVisualToTune(CarVisual carVisual)
        {
            _carVisual = carVisual;
        }

        private int ChangeVisuals(CharType characteristics, float value)
        {
            switch (characteristics)
            {
                case CharType.Speed:
                    //TuneSpeed(value);
                    TuneVisual(CharType.Speed, value);
                    break;
                case CharType.Mobility:
                    //TuneMobility(value);
                    TuneVisual(CharType.Mobility, value);
                    break;
                case CharType.Durability:
                    //TuneDurability(value);
                    TuneVisual(CharType.Durability, value);
                    break;
                case CharType.Acceleration:
                    //TuneAcceleration(value);
                    TuneVisual(CharType.Acceleration, value);
                    break;
            }

            _playerCarDepot.UpdateProfile(_carProfile);

            return _carProfile.CarCharacteristics.AvailableFactorsToUse;
        }

        //private void TuneSpeed(float value)
        //{
        //    int newValue;
        //    bool canSet = CanSetValue
        //        (
        //        _carProfile.CarCharacteristics.CurrentSpeedFactor,
        //        Mathf.RoundToInt(value),
        //        _carProfile.CarCharacteristics.AvailableFactorsToUse,
        //        out newValue
        //        );

        //    if (!canSet)
        //    {
        //        OnCharacteristicValueLimit?.Invoke(CarCharacteristicsType.Speed, newValue);
        //        Debug.Log($"Can't set value: {value}, new value => {newValue}");
        //    }

        //    _carProfile.CarCharacteristics.CurrentSpeedFactor = newValue;
        //    PartLevel level = GetPartLevel(CarCharacteristicsType.Speed);

        //    var carConfigVisual = _carProfile.CarConfigVisual;
        //    carConfigVisual.CurrentWheelsLevel = level;
        //    _carVisual.SetPartsVisual(PartType.Wheel, carConfigVisual.CurrentWheelsLevel, carConfigVisual.CurrentWheelsSetType);

        //    //_playerCarDepot.UpdateProfile(_carProfile);
        //}

        //private void TuneMobility(float value)
        //{
        //    int newValue;
        //    bool canSet = CanSetValue
        //        (
        //        _carProfile.CarCharacteristics.CurrentMobilityFactor,
        //        Mathf.RoundToInt(value),
        //        _carProfile.CarCharacteristics.AvailableFactorsToUse,
        //        out newValue
        //        );

        //    if (!canSet)
        //    {
        //        OnCharacteristicValueLimit?.Invoke(CarCharacteristicsType.Mobility, newValue);
        //        Debug.Log($"Can't set value: {value}, new value => {newValue}");
        //    }

        //    _carProfile.CarCharacteristics.CurrentMobilityFactor = newValue;
        //    PartLevel level = GetPartLevel(CarCharacteristicsType.Mobility);

        //    var carConfigVisual = _carProfile.CarConfigVisual;
        //    carConfigVisual.CurrentSuspentionLevel = level;
        //    _carVisual.SetPartsVisual(PartType.Suspention, carConfigVisual.CurrentSuspentionLevel);

        //    //_playerCarDepot.UpdateProfile(_carProfile);
        //}

        //private void TuneDurability(float value)
        //{
        //    int newValue;
        //    bool canSet = CanSetValue
        //        ( 
        //        _carProfile.CarCharacteristics.CurrentDurabilityFactor,
        //        Mathf.RoundToInt(value),
        //        _carProfile.CarCharacteristics.AvailableFactorsToUse,
        //        out newValue
        //        );

        //    if (!canSet)
        //    {
        //        OnCharacteristicValueLimit?.Invoke(CarCharacteristicsType.Durability, newValue);
        //        Debug.Log($"Can't set value: {value}, new value => {newValue}");
        //    }

        //    _carProfile.CarCharacteristics.CurrentDurabilityFactor = newValue;
        //    PartLevel level = GetPartLevel(CarCharacteristicsType.Durability);

        //    var carConfigVisual = _carProfile.CarConfigVisual;
        //    carConfigVisual.CurrentBumpersLevel = level;
        //    _carVisual.SetPartsVisual(PartType.Bumper, carConfigVisual.CurrentBumpersLevel);

        //    //_playerCarDepot.UpdateProfile(_carProfile);
        //}

        //private void TuneAcceleration(float value)
        //{
        //    int newValue;
        //    bool canSet = CanSetValue
        //        (
        //        _carProfile.CarCharacteristics.CurrentAccelerationFactor,
        //        Mathf.RoundToInt(value),
        //        _carProfile.CarCharacteristics.AvailableFactorsToUse,
        //        out newValue
        //        );

        //    if (!canSet)
        //    {
        //        OnCharacteristicValueLimit?.Invoke(CarCharacteristicsType.Acceleration, newValue);
        //        Debug.Log($"Can't set value: {value}, new value => {newValue}");
        //    }

        //    _carProfile.CarCharacteristics.CurrentAccelerationFactor = newValue;
        //    PartLevel level = GetPartLevel(CarCharacteristicsType.Acceleration);

        //    var carConfigVisual = _carProfile.CarConfigVisual;
        //    carConfigVisual.CurrentBodyKitsLevel = level;
        //    _carVisual.SetPartsVisual(PartType.BodyKit, carConfigVisual.CurrentBodyKitsLevel);

        //   // _playerCarDepot.UpdateProfile(_carProfile);
        //}

        private void TuneVisual(CharType cType, float value)
        {
            var characteristics = _carProfile.CarCharacteristics;

            int currentFactor = cType switch
            {
                CharType.Speed => characteristics.CurrentSpeedFactor,
                CharType.Mobility => characteristics.CurrentMobilityFactor,
                CharType.Durability => characteristics.CurrentDurabilityFactor,
                CharType.Acceleration => characteristics.CurrentAccelerationFactor,
                _ => 0
            };

            //int newValue;
            int available = _carProfile.CarCharacteristics.AvailableFactorsToUse;
            bool canSet = CanSetValue(currentFactor, Mathf.RoundToInt(value), available, out int newValue);

            if (!canSet)
            {
                //int available = _carProfile.CarCharacteristics.AvailableFactorsToUse;
                OnCharValueLimit?.Invoke(new TuneData { cType = cType, value = newValue, available = available }) ;
            }

            _ = cType switch
            {
                CharType.Speed => _carProfile.CarCharacteristics.CurrentSpeedFactor = newValue,
                CharType.Mobility => _carProfile.CarCharacteristics.CurrentMobilityFactor = newValue,
                CharType.Durability => _carProfile.CarCharacteristics.CurrentDurabilityFactor = newValue,
                CharType.Acceleration => _carProfile.CarCharacteristics.CurrentAccelerationFactor = newValue,
                _ => 0
            };

            PartLevel newLevel = GetPartLevel(cType);

            var carConfigVisual = _carProfile.CarConfigVisual;

            PartLevel currentLevel = cType switch
            {
                CharType.Speed => carConfigVisual.CurrentWheelsLevel = newLevel,
                CharType.Mobility => carConfigVisual.CurrentSuspentionLevel = newLevel,
                CharType.Durability => carConfigVisual.CurrentBumpersLevel = newLevel,
                CharType.Acceleration => carConfigVisual.CurrentBodyKitsLevel = newLevel,
                _ => throw new NotImplementedException()
            };

            (PartType, PartLevel) d = cType switch
            {
                CharType.Speed => (PartType.Wheel, currentLevel),
                CharType.Mobility => (PartType.Suspention, currentLevel),
                CharType.Durability => (PartType.Bumper, currentLevel),
                CharType.Acceleration => (PartType.BodyKit, currentLevel),
                _ => throw new NotImplementedException()
            };

            WheelsSetType wst = carConfigVisual.CurrentWheelsSetType;
            _carVisual.SetPartsVisual(d.Item1, d.Item2, wst);
        }

        private bool CanSetValue(int oldValue, int newValue, int maxDifference, out int outValue)
        {
            if (newValue - oldValue > maxDifference)
            {
                outValue = oldValue + maxDifference;
                return false;
            }
            
            outValue = newValue;
            return true;
        }

        private PartLevel GetPartLevel(CharType characteristics)
        {
            var c = _carProfile.CarCharacteristics;
            int percentage = characteristics switch
            {
                CharType.Speed => Mathf.RoundToInt(c.CurrentSpeedFactor * 100 / c.MaxSpeedFactor),
                CharType.Mobility => Mathf.RoundToInt(c.CurrentMobilityFactor * 100 / c.MaxMobilityFactor),
                CharType.Durability => Mathf.RoundToInt(c.CurrentDurabilityFactor * 100 / c.MaxDurabilityFactor),
                CharType.Acceleration => Mathf.RoundToInt(c.CurrentAccelerationFactor * 100 / c.MaxAccelerationFactor),
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

            //Debug.Log($"CHAR: {characteristics} => PERCENTAGE: {percentage} => LEVEL: {pLevel}");

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

    public struct TuneData
    {
        public CharType cType;
        public int value;
        public int available;
    }
}