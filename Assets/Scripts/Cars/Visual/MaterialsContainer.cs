using Newtonsoft.Json;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    [CreateAssetMenu(menuName = "Containers/MaterialsContainer", fileName = "MaterialsContainer", order = 1)]
    public class MaterialsContainer : SerializedScriptableObject
    {
        [SerializeField]
        private List<CarMaterial> CarMaterials = new List<CarMaterial>();

        public Material GetMaterialTypeOf(CarName carName, MaterialSetType partsSetType)
        {
            var carMat = CarMaterials.Find(c => c.carName == carName);
            var holder = carMat.materialHolders.Find(mat => mat.PartsSetType == partsSetType);
            return holder.Material;
        }

        //public void SetMaterialsAccessibility(CarName carName, MaterialSetType materialSetType)
        //{
        //    CarMaterials.Find(c => c.carName == carName)
        //        .materialHolders.Find(mat => mat.PartsSetType == materialSetType)
        //        .isAvailable = true;
        //}

        [Serializable]
        public class CarMaterial
        { 
            public CarName carName;
            public List<MaterialHolder> materialHolders = new List<MaterialHolder>();
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
