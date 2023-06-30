using System;
using UnityEngine;
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

                    float handlindFactor = CalculateValueFactor
                        (
                            _opponentsTuneScheme.MaxHandlingPercentage,
                            _opponentsTuneScheme.MinHandlingPercentageCurrent,
                            _opponentsTuneScheme.HandlingPercentageValueRange
                        );

                    //Debug.Log($"Handling factor: {handlindFactor}");

                    float mobilityValue = oProfile.CarCharacteristics.MaxHandlingFactor * handlindFactor;

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Handling, mobilityValue, !randomizeView);
                }

                if (_opponentsTuneScheme.UseFrictionAdjust)
                { 
                    oProfile.Friction = pProfile.Friction;
                    float frictionFactor = CalculateValueFactor
                        (
                            _opponentsTuneScheme.MaxFrictionPercentage,
                            _opponentsTuneScheme.MinFrictionPercentage,
                            _opponentsTuneScheme.FrictionPercentageValueRange
                        );

                    //Debug.Log($"Grip factor: {frictionFactor}");

                    float frictionValue = oProfile.CarCharacteristics.MaxFrictionFactor * frictionFactor;

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Friction, frictionValue, !randomizeView);
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
                _opponentsTuneScheme.MinSpeedPercentageCurrent = 
                    RecalculateValue
                    (
                        _opponentsTuneScheme.MinSpeedPercentageCurrent,
                        _opponentsTuneScheme.MinSpeedPercentage,
                        _opponentsTuneScheme.MaxSpeedPercentage,
                        _opponentsTuneScheme.VictoryPercentageStep,
                        _opponentsTuneScheme.LoosePercentageStep,
                        grade
                    );
            }

            if (_opponentsTuneScheme.UseHandlingAdjust)
            {
                _opponentsTuneScheme.MinHandlingPercentageCurrent =
                    RecalculateValue
                    (
                        _opponentsTuneScheme.MinHandlingPercentageCurrent,
                        _opponentsTuneScheme.MinHandlingPercentage,
                        _opponentsTuneScheme.MaxHandlingPercentage,
                        _opponentsTuneScheme.VictoryPercentageStep,
                        _opponentsTuneScheme.LoosePercentageStep,
                        grade
                    );
            }

            if (_opponentsTuneScheme.UseAccelerationAdjust)
            {
                _opponentsTuneScheme.MinAccelerationPercentageCurrent =
                    RecalculateValue
                    (
                        _opponentsTuneScheme.MinAccelerationPercentageCurrent,
                        _opponentsTuneScheme.MinAccelerationPercentage,
                        _opponentsTuneScheme.MaxAccelerationPercentage,
                        _opponentsTuneScheme.VictoryPercentageStep,
                        _opponentsTuneScheme.LoosePercentageStep,
                        grade
                    );
            }

            if (_opponentsTuneScheme.UseFrictionAdjust)
            {
                _opponentsTuneScheme.MinFrictionPercentageCurrent = 
                    RecalculateValue
                    (
                        _opponentsTuneScheme.MinFrictionPercentageCurrent,
                        _opponentsTuneScheme.MinFrictionPercentage,
                        _opponentsTuneScheme.MaxFrictionPercentage,
                        _opponentsTuneScheme.VictoryPercentageStep,
                        _opponentsTuneScheme.LoosePercentageStep,
                        grade
                    );
            }

            if (_opponentsTuneScheme.UseDurabilityAdjust)
            {
                _opponentsTuneScheme.MinDurabilityPercentageCurrent =
                    RecalculateValue
                    (
                        _opponentsTuneScheme.MinDurabilityPercentageCurrent,
                        _opponentsTuneScheme.MinDurabilityPercentage,
                        _opponentsTuneScheme.MaxDurabilityPercentage,
                        _opponentsTuneScheme.VictoryPercentageStep,
                        _opponentsTuneScheme.LoosePercentageStep,
                        grade
                    );
            }
        }

        private float RecalculateValue(float curMinVal, float minVal, float maxVal, float upGradeStep, float downGradeStep, bool doGrade)
        {
            if (doGrade)
            {
                curMinVal += upGradeStep;
                if(curMinVal > maxVal)
                    curMinVal = maxVal;
            }
            else
            { 
                curMinVal -= downGradeStep;
                if(curMinVal < minVal)
                    curMinVal = minVal;
            }

            return curMinVal;
        }

        public void Dispose()
        {
            _tuner.Dispose();
        }
    }
}