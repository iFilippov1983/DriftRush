using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarVisual : MonoBehaviour
    {
        [SerializeField] private CarName _carName;
        [SerializeField] private CarBody _carBody;
        [SerializeField] private List<WheelsSet> _wheelsSets = new List<WheelsSet>();
        [SerializeField] private SuspentionSet _suspentionSet = new SuspentionSet();
        [SerializeField] private BumperSet _bumpersSet = new BumperSet();
        [SerializeField] private BodyKitSet _bodyKitsSet = new BodyKitSet();

        private CarConfigVisual _carConfigVisual;
        private MaterialsContainer _materialsContainer;

        public CarBody CarBody => _carBody;
        public CarName CarName => _carName;

        public void Initialize(CarConfigVisual carConfigVisual, MaterialsContainer materialsContainer)
        { 
            _carConfigVisual = carConfigVisual;
            _materialsContainer = materialsContainer;
            _carConfigVisual.SetMaterials(_materialsContainer);
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            //Debug.Log($"Initial Apply => " +
            //    $"\nMat type: {_carConfigVisual.CurrentMaterialsSetType}; " +
            //    $"\nWheels level: {_carConfigVisual.CurrentWheelsLevel}; " +
            //    $"\nSusp level: {_carConfigVisual.CurrentSuspentionLevel}; " +
            //    $"\nBumpers level: {_carConfigVisual.CurrentBumpersLevel}; " +
            //    $"\nBody kits level: {_carConfigVisual.CurrentBodyKitsLevel}");

            SetBodyMaterials(_carConfigVisual.CurrentMaterialsSetType);
            SetPartsVisual(PartType.Wheel, _carConfigVisual.CurrentWheelsLevel, _carConfigVisual.CurrentWheelsSetType);
            SetPartsVisual(PartType.Suspention, _carConfigVisual.CurrentSuspentionLevel);
            SetPartsVisual(PartType.Bumper, _carConfigVisual.CurrentBumpersLevel);
            SetPartsVisual(PartType.BodyKit,  _carConfigVisual.CurrentBodyKitsLevel);
        }

        public void SetBodyMaterials(MaterialSetType materialsSetType)
        {
            _carConfigVisual.CurrentMaterialsSetType = materialsSetType;
            CarBody.SetMaterial(_carConfigVisual.GetCurrentMaterial());
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
                    HandleSuspentionTune(partLevel);
                    break;
                case PartType.Bumper:
                    HandleBumpersTune(partLevel);
                    break;
                case PartType.BodyKit:
                    HandleBodyKitsTune(partLevel);
                    break;
            }
        }

        private void HandleWheelTune(PartLevel partLevel, WheelsSetType wheelsSetType = WheelsSetType.Default)
        {
            var set = _wheelsSets.Find(s => s.WheelsSetType == wheelsSetType);

            if (wheelsSetType != _carConfigVisual.CurrentWheelsSetType)
            {
                var previousSet = _wheelsSets.Find(s => s.WheelsSetType == _carConfigVisual.CurrentWheelsSetType);
                previousSet.UnInstall();
                set.Install();
            }

            set.SetPartsLevel(partLevel);
            _carConfigVisual.CurrentWheelsSetType = wheelsSetType;
            _carConfigVisual.CurrentWheelsLevel = partLevel;
        }

        private void HandleSuspentionTune(PartLevel partLevel)
        {
            _suspentionSet.SetPartsLevel(partLevel);
            _carConfigVisual.CurrentSuspentionLevel = partLevel;
        }

        private void HandleBumpersTune(PartLevel partLevel)
        {
            _bumpersSet.SetPartsLevel(partLevel);
            _carConfigVisual.CurrentBumpersLevel = partLevel;
        }

        private void HandleBodyKitsTune(PartLevel partLevel)
        {
            _bodyKitsSet.SetPartsLevel(partLevel);
            _carConfigVisual.CurrentBodyKitsLevel = partLevel;
        }
    }
}