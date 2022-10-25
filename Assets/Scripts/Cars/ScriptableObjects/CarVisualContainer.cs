using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Cars/CarVisualContainer", fileName = "CarVisualContainer", order = 1)]
    public class CarVisualContainer : SerializedScriptableObject
    {
        public DriverType DriverType;
        public CarName CarName;

        [FoldoutGroup("Materials Settings")]
        public PartsSetType CurrentMaterialsSetType;

        [SerializeField]
        [FoldoutGroup("Materials Settings")]
        [DictionaryDrawerSettings(KeyLabel = "Set Type", ValueLabel = "Materials")]
        private Dictionary<PartsSetType, List<Material>> Materials = new Dictionary<PartsSetType, List<Material>>()
        {
            { PartsSetType.Default, new List<Material>() },
            { PartsSetType.Military, new List<Material>() },
        };

        [FoldoutGroup("Wheels Settings")]
        public PartsSetType CurrentWheelsSetType;

        [FoldoutGroup("Wheels Settings")]
        public PartLevel CurrentWheelsLevel;

        [FoldoutGroup("Suspention Settings")]
        public PartLevel CurrentSuspentionLevel;

        [FoldoutGroup("Bumpers Settings")]
        public PartLevel CurrentBumpersLevel;

        [FoldoutGroup("Body Kits Settings")]
        public PartLevel CurrentBodyKitsLevel;

        public List<Material> CurrentMaterials => Materials[CurrentMaterialsSetType];

    }
}
