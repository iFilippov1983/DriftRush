using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarVisual : MonoBehaviour
    {
        public CarConfigVisual CarConfigVisual;

        [SerializeField] private CarName _carName;
        [SerializeField] private CarBody _carBody;
        [SerializeField] private List<WheelsSet> _wheelsSets = new List<WheelsSet>();
        [SerializeField] private SuspentionSet _suspentionSet = new SuspentionSet();
        [SerializeField] private BumperSet _bumpersSet = new BumperSet();
        [SerializeField] private BodyKitSet _bodyKitsSet = new BodyKitSet();

        public CarBody CarBody => _carBody;
        public CarName CarName => _carName;

        public void ApplyVisual()
        { 
            SetBodyMaterials(CarConfigVisual.CurrentMaterialsSetType);
            SetPartsVisual(PartType.Wheel, CarConfigVisual.CurrentWheelsLevel, CarConfigVisual.CurrentWheelsSetType);
            SetPartsVisual(PartType.Suspention, CarConfigVisual.CurrentSuspentionLevel);
            SetPartsVisual(PartType.Bumper, CarConfigVisual.CurrentBumpersLevel);
            SetPartsVisual(PartType.BodyKit,  CarConfigVisual.CurrentBodyKitsLevel);
        }

        public void SetBodyMaterials(MaterialSetType materialsSetType)
        {
            CarConfigVisual.CurrentMaterialsSetType = materialsSetType;
            CarBody.SetMaterial(CarConfigVisual.GetCurrentMaterial());
        }

        [Button]
        public void SetPartsVisual(PartType partType, PartLevel partLevel, WheelsSetType wheelsSetType = WheelsSetType.Default)
        {
            switch (partType)
            {
                case PartType.Wheel:
                    HandleWheelTune(partLevel, wheelsSetType);
                    break;
                case PartType.Suspention:
                    _suspentionSet.SetPartsLevel(partLevel);
                    CarConfigVisual.CurrentSuspentionLevel = partLevel;
                    break;
                case PartType.Bumper:
                    _bumpersSet.SetPartsLevel(partLevel);
                    CarConfigVisual.CurrentBumpersLevel = partLevel;
                    break;
                case PartType.BodyKit:
                    _bodyKitsSet.SetPartsLevel(partLevel);
                    CarConfigVisual.CurrentBodyKitsLevel = partLevel;
                    break;
            }
        }

        private void HandleWheelTune(PartLevel partLevel, WheelsSetType wheelsSetType = WheelsSetType.Default)
        {
            $"Handling wheel tune: Level {partLevel}; Type: {wheelsSetType}".Log(ConsoleLog.Color.Yellow);

            var set = _wheelsSets.Find(s => s.WheelsSetType == wheelsSetType);

            if (wheelsSetType != CarConfigVisual.CurrentWheelsSetType)
            {
                var previousSet = _wheelsSets.Find(s => s.WheelsSetType == CarConfigVisual.CurrentWheelsSetType);
                previousSet.UnInstall();
                set.Install();
            }

            set.SetPartsLevel(partLevel);
            CarConfigVisual.CurrentWheelsSetType = wheelsSetType;
            CarConfigVisual.CurrentWheelsLevel = partLevel;
        }
    }
}