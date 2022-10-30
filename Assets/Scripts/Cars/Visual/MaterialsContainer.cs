using Newtonsoft.Json;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    [CreateAssetMenu(menuName = "Cars/MaterialsContainer", fileName = "MaterialsContainer", order = 1)]
    public class MaterialsContainer : SerializedScriptableObject
    {
        [SerializeField]
        [FoldoutGroup("Materials Settings")]
        [DictionaryDrawerSettings(KeyLabel = "Set Type", ValueLabel = "Materials")]
        private Dictionary<CarName, List<MaterialHolder>> MaterialHolders = new Dictionary<CarName, List<MaterialHolder>>();

        public List<MaterialHolder> GetHoldersFor(CarName carName) => 
            MaterialHolders[carName];

        public Material GetMaterialTypeOf(CarName carName, MaterialSetType partsSetType) =>
            MaterialHolders[carName].Find(h => h.PartsSetType == partsSetType).Material;

        public bool TryGetMaterial(CarName carName, MaterialSetType partsSetType, out Material material)
        {
            var holder = MaterialHolders[carName].Find(h => h.PartsSetType == partsSetType);

            if (holder != null && holder.isAvailable)
            {
                material = holder.Material;
                return true;
            }
            else
            {
                material = null;
                return false;
            }
        }

        [Serializable]
        public class MaterialHolder
        {
            public bool isAvailable;
            public MaterialSetType PartsSetType;
            public MaterialName MaterialName;

            [SerializeField]
            private Material _material;

            public Material Material
            {
                get
                {
                    if (_material == null)
                        _material = ResourcesLoader.LoadObject<Material>(ResourcePath.MaterialsPrefabsFolder + MaterialName);

                    return _material;
                }
            }
        }
    }
}
