using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Newtonsoft.Json;

namespace RaceManager.Cars
{
    [Serializable]
    public class CarConfigVisual
    {
        private MaterialsContainer _materialsContainer;
        private Dictionary<MaterialSetType, Material> _materials;

        public DriverType DriverType;
        public CarName CarName;

        [FoldoutGroup("Materials Settings")]
        public MaterialSetType CurrentMaterialsSetType = MaterialSetType.Default;

        [SerializeField]
        [JsonProperty]
        [FoldoutGroup("Materials Settings")]
        private List<MaterialSetType> _availableMaterialSets = new List<MaterialSetType>();

        [FoldoutGroup("Wheels Settings")]
        public WheelsSetType CurrentWheelsSetType = WheelsSetType.Default;

        [FoldoutGroup("Wheels Settings")]
        public PartLevel CurrentWheelsLevel = PartLevel.First;

        [FoldoutGroup("Suspention Settings")]
        public PartLevel CurrentSuspentionLevel = PartLevel.First;

        [FoldoutGroup("Bumpers Settings")]
        public PartLevel CurrentBumpersLevel = PartLevel.First;

        [FoldoutGroup("Body Kits Settings")]
        public PartLevel CurrentBodyKitsLevel = PartLevel.First;

        public Material GetCurrentMaterial() => _materials[CurrentMaterialsSetType];

        public void AddAvailableMaterial(MaterialSetType materialSetType)
        {
            _availableMaterialSets.Add(materialSetType);
            SetMaterials(_materialsContainer);
        }

        public void SetMaterials(MaterialsContainer container)
        {
            _materialsContainer = container;
            _materials = new Dictionary<MaterialSetType, Material>();
            foreach (var setType in _availableMaterialSets)
            {
                _materials.Add(setType, container.GetMaterialTypeOf(CarName, setType));
            }
        }
    }
}
