using System;
using Random = UnityEngine.Random;

namespace RaceManager.Cars
{
    public class OpponentsCarTuner : IDisposable
    {
        private CarTuner _tuner;
        private CarsDepot _playerCarDepot;
        private CarsDepot _opponentsCarDepot;
        private OpponentsTuneScheme _opponentsTuneScheme;

        //Injected
        public OpponentsCarTuner(CarsDepot playerCarDepot, OpponentsTuneScheme opponentsTuneScheme)
        {
            _playerCarDepot = playerCarDepot;
            _opponentsTuneScheme = opponentsTuneScheme;
        }

        public bool CanAdjust => _opponentsTuneScheme.AdjustFromStart;
        public int CanAdjustThreshold => _opponentsTuneScheme.VictoriesThreshold;

        public void Initialize(CarsDepot opponentsCarDepot, bool gradePercentage)
        {
            GradePercentage(gradePercentage);

            _opponentsCarDepot = opponentsCarDepot;
            _tuner = new CarTuner(_opponentsCarDepot);
        }

        public void AdjustOpponentsCarDepot()
        {
            CarProfile pProfile = _playerCarDepot.CurrentCarProfile;
            bool randomizeView = _opponentsTuneScheme.RandomizeOpponentsView;

            for (int i = 0; i < _opponentsCarDepot.ProfilesList.Count; i++)
            {
                CarProfile oProfile = _opponentsCarDepot.ProfilesList[i];

                _opponentsCarDepot.CurrentCarName = oProfile.CarName;
                _tuner.SetCarProfile();

                if (_opponentsTuneScheme.UseSpeedAdjust)
                {
                    oProfile.Speed = pProfile.Speed;

                    float speedFactor = CalculateValueFactor
                        (
                            _opponentsTuneScheme.MaxSpeedPercentage,
                            _opponentsTuneScheme.MinSpeedPercentageCurrent,
                            _opponentsTuneScheme.SpeedPercentageValueRange
                        );

                    //Debug.Log($"Speed factor: {speedFactor}");

                    float speedValue = oProfile.CarCharacteristics.MaxSpeedFactor * speedFactor;

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Speed, speedValue, !randomizeView);
                }


                if (_opponentsTuneScheme.UseHandlingAdjust)
                {
                    oProfile.Handling = pProfile.Handling;

                    float mobilityFactor = CalculateValueFactor
                        (
                            _opponentsTuneScheme.MaxHandlingPercentage,
                            _opponentsTuneScheme.MinHandlingPercentageCurrent,
                            _opponentsTuneScheme.HandlingPercentageValueRange
                        );

                    //Debug.Log($"Mobility factor: {mobilityFactor}");

                    float mobilityValue = oProfile.CarCharacteristics.MaxHandlingFactor * mobilityFactor;

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Handling, mobilityValue, !randomizeView);
                }

                if (_opponentsTuneScheme.UseFrictionAdjust)
                { 
                    oProfile.Friction = pProfile.Friction;
                    float gripFactor = CalculateValueFactor
                        (
                            _opponentsTuneScheme.MaxFrictionPercentage,
                            _opponentsTuneScheme.MinFrictionPercentage,
                            _opponentsTuneScheme.FrictionPercentageValueRange
                        );

                    //Debug.Log($"Grip factor: {mobilityFactor}");

                    float gripValue = oProfile.CarCharacteristics.MaxFrictionFactor * gripFactor;

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Friction, gripValue, !randomizeView);
                }

                if (_opponentsTuneScheme.UseAccelerationAdjust)
                {
                    oProfile.Acceleration = pProfile.Acceleration;

                    float accelerationFactor = CalculateValueFactor
                        (
                            _opponentsTuneScheme.MaxAccelerationPercentage,
                            _opponentsTuneScheme.MinAccelerationPercentageCurrent,
                            _opponentsTuneScheme.AccelerationPercentageValueRange
                        );

                    //Debug.Log($"Acceleration factor: {accelerationFactor}");

                    float accelerationValue = oProfile.CarCharacteristics.MaxAccelerationFactor * accelerationFactor;

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Acceleration, accelerationValue, !randomizeView);
                }

                if (_opponentsTuneScheme.UseDurabilityAdjust)
                {
                    oProfile.Durability = pProfile.Durability;

                    float durabilityFactor = CalculateValueFactor
                        (
                            _opponentsTuneScheme.MaxDurabilityPercentage,
                            _opponentsTuneScheme.MinDurabilityPercentageCurrent,
                            _opponentsTuneScheme.DurabilityPercentageValueRange
                        );

                    //Debug.Log($"Durability factor: {durabilityFactor}");

                    float durabilityValue = oProfile.CarCharacteristics.MaxDurabilityFactor * durabilityFactor;

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Durability, durabilityValue, !randomizeView);
                }
            }
        }

        public void AdjustOpponentsCarView(CarVisual carVisual)
        {
            if (!_opponentsTuneScheme.RandomizeOpponentsView)
                return;

            _tuner.SetTuner(carVisual);
            _tuner.SetCarProfile();
            _tuner.SetRandomCarView();
        }

        private float CalculateValueFactor(float maxPerc, float minPerc, float valueRange)
        { 
            if(minPerc > maxPerc)
                minPerc = maxPerc;

            float maxPercCurrent = minPerc + valueRange;
            if (maxPercCurrent > maxPerc)
                maxPercCurrent = maxPerc;

            float factor = Random.Range(minPerc, maxPercCurrent);
            return factor;
        }

        private void GradePercentage(bool grade)
        {
            if (_opponentsTuneScheme.UseSpeedAdjust)
            {
                if (grade)
                {
                    _opponentsTuneScheme.MinSpeedPercentageCurrent += _opponentsTuneScheme.VictoryPercentageStep;
                    if (_opponentsTuneScheme.MinSpeedPercentageCurrent > _opponentsTuneScheme.MaxSpeedPercentage)
                        _opponentsTuneScheme.MinSpeedPercentageCurrent = _opponentsTuneScheme.MaxSpeedPercentage;
                }
                else
                {
                    _opponentsTuneScheme.MinSpeedPercentageCurrent -= _opponentsTuneScheme.LoosePercentageStep;
                    if (_opponentsTuneScheme.MinSpeedPercentageCurrent < _opponentsTuneScheme.MinSpeedPercentage)
                        _opponentsTuneScheme.MinSpeedPercentageCurrent = _opponentsTuneScheme.MinSpeedPercentage;
                }
            }

            if (_opponentsTuneScheme.UseHandlingAdjust)
            {
                if (grade)
                {
                    _opponentsTuneScheme.MinHandlingPercentageCurrent += _opponentsTuneScheme.VictoryPercentageStep;
                    if (_opponentsTuneScheme.MinHandlingPercentageCurrent > _opponentsTuneScheme.MaxHandlingPercentage)
                        _opponentsTuneScheme.MinHandlingPercentageCurrent = _opponentsTuneScheme.MaxHandlingPercentage;
                }
                else
                {
                    _opponentsTuneScheme.MinHandlingPercentageCurrent -= _opponentsTuneScheme.LoosePercentageStep;
                    if (_opponentsTuneScheme.MinHandlingPercentageCurrent < _opponentsTuneScheme.MinHandlingPercentage)
                        _opponentsTuneScheme.MinHandlingPercentageCurrent = _opponentsTuneScheme.MinHandlingPercentage;
                }
            }

            if (_opponentsTuneScheme.UseDurabilityAdjust)
            {
                if (grade)
                {
                    _opponentsTuneScheme.MinDurabilityPercentageCurrent += _opponentsTuneScheme.VictoryPercentageStep;
                    if (_opponentsTuneScheme.MinDurabilityPercentageCurrent > _opponentsTuneScheme.MaxDurabilityPercentage)
                        _opponentsTuneScheme.MinDurabilityPercentageCurrent = _opponentsTuneScheme.MaxDurabilityPercentage;
                }
                else
                {
                    _opponentsTuneScheme.MinDurabilityPercentageCurrent -= _opponentsTuneScheme.LoosePercentageStep;
                    if (_opponentsTuneScheme.MinDurabilityPercentageCurrent < _opponentsTuneScheme.MinDurabilityPercentage)
                        _opponentsTuneScheme.MinDurabilityPercentageCurrent = _opponentsTuneScheme.MinDurabilityPercentage;
                }
            }

            if (_opponentsTuneScheme.UseAccelerationAdjust)
            {
                if (grade)
                {
                    _opponentsTuneScheme.MinAccelerationPercentageCurrent += _opponentsTuneScheme.VictoryPercentageStep;
                    if (_opponentsTuneScheme.MinAccelerationPercentageCurrent > _opponentsTuneScheme.MaxAccelerationPercentage)
                        _opponentsTuneScheme.MinAccelerationPercentageCurrent = _opponentsTuneScheme.MaxAccelerationPercentage;
                }
                else
                {
                    _opponentsTuneScheme.MinAccelerationPercentageCurrent -= _opponentsTuneScheme.LoosePercentageStep;
                    if (_opponentsTuneScheme.MinAccelerationPercentageCurrent < _opponentsTuneScheme.MinAccelerationPercentage)
                        _opponentsTuneScheme.MinAccelerationPercentageCurrent = _opponentsTuneScheme.MinAccelerationPercentage;
                }
            }
        }

        public void Dispose()
        {
            _tuner.Dispose();
        }
    }
}