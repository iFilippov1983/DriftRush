using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RaceManager.Cars
{
    public class OpponentsCarTuner
    {
        private CarTuner _tuner;
        private CarsDepot _playerCarDepot;
        private OpponentsTuneScheme _opponentsTuneScheme;

        //Injected
        public OpponentsCarTuner(CarsDepot playerCarDepot, OpponentsTuneScheme opponentsTuneScheme)
        {
            _playerCarDepot = playerCarDepot;
            _opponentsTuneScheme = opponentsTuneScheme;
        }

        public bool CanAdjust => _opponentsTuneScheme.AdjustFromStart;
        public int CanAdjustThreshold => _opponentsTuneScheme.VictoriesThreshold;

        public void AdjustOpponentsCarDepot(CarsDepot opponentsCarDepot, bool gradePercentage)
        {
            GradePercentage(gradePercentage);

            bool randomizeView = _opponentsTuneScheme.RandomizeOpponentsView;

            _tuner = new CarTuner(opponentsCarDepot);
            CarProfile pProfile = _playerCarDepot.CurrentCarProfile;

            for (int i = 0; i < opponentsCarDepot.ProfilesList.Count; i++)
            {
                CarProfile oProfile = opponentsCarDepot.ProfilesList[i];

                opponentsCarDepot.CurrentCarName = oProfile.CarName;
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

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Speed, speedFactor, !randomizeView);
                }


                if (_opponentsTuneScheme.UseMobilityAdjust)
                {
                    oProfile.Mobility = pProfile.Mobility;

                    float mobilityFactor = CalculateValueFactor
                        (
                            _opponentsTuneScheme.MaxMobilityPercentage,
                            _opponentsTuneScheme.MinMobilityPercentageCurrent,
                            _opponentsTuneScheme.MobilityPercentageValueRange
                        );

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Mobility, mobilityFactor, !randomizeView);
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

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Durability, durabilityFactor, !randomizeView);
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

                    _tuner.OnCharacteristicValueChanged?.Invoke(CharacteristicType.Acceleration, accelerationFactor, !randomizeView);
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

            if (_opponentsTuneScheme.UseMobilityAdjust)
            {
                if (grade)
                {
                    _opponentsTuneScheme.MinMobilityPercentageCurrent += _opponentsTuneScheme.VictoryPercentageStep;
                    if (_opponentsTuneScheme.MinMobilityPercentageCurrent > _opponentsTuneScheme.MaxMobilityPercentage)
                        _opponentsTuneScheme.MinMobilityPercentageCurrent = _opponentsTuneScheme.MaxMobilityPercentage;
                }
                else
                {
                    _opponentsTuneScheme.MinMobilityPercentageCurrent -= _opponentsTuneScheme.LoosePercentageStep;
                    if (_opponentsTuneScheme.MinMobilityPercentageCurrent < _opponentsTuneScheme.MinMobilityPercentage)
                        _opponentsTuneScheme.MinMobilityPercentageCurrent = _opponentsTuneScheme.MinMobilityPercentage;
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
    }
}