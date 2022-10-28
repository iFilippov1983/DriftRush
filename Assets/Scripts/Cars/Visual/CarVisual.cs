using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarVisual : MonoBehaviour
    {
        public CarVisualContainer CarVisualContainer;
        [SerializeField] 
        private CarConfigVisual _carConfigVisual;

        private PartConfig _currentWheelsPartConfig;
        private PartConfig _currentSuspentionPartConfig;
        private PartConfig _currentBodyKitsPartConfig;
        private PartConfig _currentBumpersPartConfig;

        //public CarVisualContainer CarVisualContainer 
        //{   get
        //    {
        //        return _carVisualContainer;
        //    }
        //    set { _carVisualContainer = value; } 
        //}

        public void ApplyVisual()
        { 
            SetBodyMaterials(CarVisualContainer.CarName, CarVisualContainer.CurrentMaterialsSetType);
            SetPartsVisual(PartType.Wheel, CarVisualContainer.CurrentWheelsSetType, CarVisualContainer.CurrentWheelsLevel);
            SetPartsVisual(PartType.Suspention, PartsSetType.Default, CarVisualContainer.CurrentSuspentionLevel);
            SetPartsVisual(PartType.Bumper, PartsSetType.Default, CarVisualContainer.CurrentBumpersLevel);
            SetPartsVisual(PartType.BodyKit, PartsSetType.Default, CarVisualContainer.CurrentBodyKitsLevel);
        }

        public float GetCurrentWheelsRadius() =>
            _currentWheelsPartConfig.CurrentProperties.Value;

        public float GetCurrentSuspentionHeight() =>
            _currentSuspentionPartConfig.CurrentProperties.Value;

        public void SetBodyMaterials(CarName carName, PartsSetType partsSetType)
        {
            CarVisualContainer.CurrentMaterialsSetType = partsSetType;
            var material = CarVisualContainer.MaterialsContainer.GetMaterialTypeOf(carName, partsSetType);
            _carConfigVisual.CarBody.SetMaterial(material);
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
                    CarVisualContainer.CurrentWheelsLevel = partLevel;
                    break;
                case PartType.Suspention:
                    _currentSuspentionPartConfig = partConfig;
                    CarVisualContainer.CurrentSuspentionLevel = partLevel;
                    break;
                case PartType.Bumper:
                    _currentBumpersPartConfig = partConfig;
                    CarVisualContainer.CurrentBumpersLevel = partLevel;
                    break;
                case PartType.BodyKit:
                    _currentBodyKitsPartConfig = partConfig;
                    CarVisualContainer.CurrentBodyKitsLevel = partLevel;
                    break;
            }
        }

        
    }
}