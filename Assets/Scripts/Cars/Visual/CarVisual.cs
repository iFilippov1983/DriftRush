using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarVisual : MonoBehaviour
    {
        public CarVisualContainer CarVisualContainer;
        //[SerializeField] 
        //private CarConfigVisual _carConfigVisual;


        [SerializeField] private CarName _carName;
        [SerializeField] private CarBody _carBody;
        [SerializeField] private List<WheelsSet> _wheelsSets = new List<WheelsSet>();
        [SerializeField] private SuspentionSet _suspentionSet = new SuspentionSet();
        [SerializeField] private BumperSet _bumpersSet = new BumperSet();
        [SerializeField] private BodyKitSet _bodyKitsSet = new BodyKitSet();
        //[SerializeField] private List<ConfigsList> _partConfigs;  

        public CarBody CarBody => _carBody;
        public CarName CarName => _carName;


        //public CarVisualContainer CarVisualContainer
        //{
        //    get
        //    {
        //        return _carVisualContainer;
        //    }
        //    set { _carVisualContainer = value; }
        //}

        public void ApplyVisual()
        { 
            SetBodyMaterials(CarVisualContainer.CarName, CarVisualContainer.CurrentMaterialsSetType);
            SetPartsVisual(PartType.Wheel, CarVisualContainer.CurrentWheelsLevel, CarVisualContainer.CurrentWheelsSetType);
            SetPartsVisual(PartType.Suspention, CarVisualContainer.CurrentSuspentionLevel);
            SetPartsVisual(PartType.Bumper, CarVisualContainer.CurrentBumpersLevel);
            SetPartsVisual(PartType.BodyKit,  CarVisualContainer.CurrentBodyKitsLevel);
        }

        //public float GetCurrentWheelsRadius() =>
        //    _currentWheelsPartSet.CurrentProperties.Value;

        //public float GetCurrentSuspentionHeight() =>
        //    _currentSuspentionPartSet.CurrentProperties.Value;

        public void SetBodyMaterials(CarName carName, MaterialSetType materialsSetType)
        {
            CarVisualContainer.CurrentMaterialsSetType = materialsSetType;
            //var material = CarVisualContainer.MaterialsContainer.GetMaterialTypeOf(carName, materialsSetType);
            //_carConfigVisual.CarBody.SetMaterial(material);
            CarBody.SetMaterial(CarVisualContainer.GetCurrentMaterial());
        }
            

        //public void SetPartsSetAvailability(PartType partType, MaterialSetType partsSetType, bool isAvailable)
        //{
        //    PartsSet partConfig = _carConfigVisual.GetPartConfig(partType, partsSetType);
        //    if (partConfig != null)
        //    { 
        //        partConfig.isAvailable = isAvailable;
        //    }
        //}

        //public bool PartSetIsAvailable(PartType partType, MaterialSetType partsSetType) =>
        //    _carConfigVisual.GetPartConfig(partType, partsSetType).isAvailable;

        [Button]
        public void SetPartsVisual(PartType partType, PartLevel partLevel, WheelsSetType wheelsSetType = WheelsSetType.Default)
        {
            switch (partType)
            {
                case PartType.Wheel:
                    var set = _wheelsSets.Find(s => s.WheelsSetType == wheelsSetType);
                    set.SetPartsLevel(partLevel);
                    CarVisualContainer.CurrentWheelsLevel = partLevel;
                    break;
                case PartType.Suspention:
                    _suspentionSet.SetPartsLevel(partLevel);
                    CarVisualContainer.CurrentSuspentionLevel = partLevel;
                    break;
                case PartType.Bumper:
                    _bumpersSet.SetPartsLevel(partLevel);
                    CarVisualContainer.CurrentBumpersLevel = partLevel;
                    break;
                case PartType.BodyKit:
                    _bodyKitsSet.SetPartsLevel(partLevel);
                    CarVisualContainer.CurrentBodyKitsLevel = partLevel;
                    break;
            }
        }

    }
}