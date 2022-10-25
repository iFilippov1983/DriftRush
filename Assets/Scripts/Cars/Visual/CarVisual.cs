using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarVisual : MonoBehaviour
    {
        [SerializeField]
        private CarVisualContainer _carVisualContainer;
        [SerializeField] 
        private CarConfigVisual _carConfigVisual;

        private PartConfig _currentWheelsPartConfig;
        private PartConfig _currentSuspentionPartConfig;
        private PartConfig _currentBodyKitsPartConfig;
        private PartConfig _currentBumpersPartConfig;

        public CarVisualContainer CarVisualContainer => _carVisualContainer;

        public void ApplyVisual(CarVisualContainer carVisualContainer)
        { 
            _carVisualContainer = carVisualContainer;

            SetBodyMaterials(_carVisualContainer.CurrentMaterialsSetType);
            SetPartsVisual(PartType.Wheel, _carVisualContainer.CurrentWheelsSetType, _carVisualContainer.CurrentWheelsLevel);
            SetPartsVisual(PartType.Suspention, PartsSetType.Default, _carVisualContainer.CurrentSuspentionLevel);
            SetPartsVisual(PartType.Bumper, PartsSetType.Default, _carVisualContainer.CurrentBumpersLevel);
            SetPartsVisual(PartType.BodyKit, PartsSetType.Default, _carVisualContainer.CurrentBodyKitsLevel);
        }

        public float GetCurrentWheelsRadius() =>
            _currentWheelsPartConfig.CurrentProperties.Value;

        public float GetCurrentSuspentionHeight() =>
            _currentSuspentionPartConfig.CurrentProperties.Value;

        public void SetBodyMaterials(PartsSetType partsSetType)
        {
            _carVisualContainer.CurrentMaterialsSetType = partsSetType;
            var materials = _carVisualContainer.CurrentMaterials;
            _carConfigVisual.CarBody.SetMaterials(materials.ToArray());
        }
            

        public void SetPartsSetAvailability(PartType partType, PartsSetType partsSetType, bool isAvailable)
        {
            PartConfig partConfig = _carConfigVisual.GetPartConfig(partType, partsSetType);
            if (partConfig != null)
            { 
                partConfig.isAvailable = isAvailable;
            }
        }

        public bool PartSetIsAvailable(PartType partType, PartsSetType partsSetType) =>
            _carConfigVisual.GetPartConfig(partType, partsSetType).isAvailable;

        [Button]
        public void SetPartsVisual(PartType partType, PartsSetType partsSetType, PartLevel partLevel)
        {
            PartConfig partConfig = _carConfigVisual.GetPartConfig(partType, partsSetType);
            
            partConfig.SetActive(partLevel);
            switch (partType)
            {
                case PartType.Wheel:
                    _currentWheelsPartConfig = partConfig;
                    _carVisualContainer.CurrentWheelsLevel = partLevel;
                    break;
                case PartType.Suspention:
                    _currentSuspentionPartConfig = partConfig;
                    _carVisualContainer.CurrentSuspentionLevel = partLevel;
                    break;
                case PartType.Bumper:
                    _currentBumpersPartConfig = partConfig;
                    _carVisualContainer.CurrentBumpersLevel = partLevel;
                    break;
                case PartType.BodyKit:
                    _currentBodyKitsPartConfig = partConfig;
                    _carVisualContainer.CurrentBodyKitsLevel = partLevel;
                    break;
            }
        }

        
    }
}