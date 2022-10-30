using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using RaceManager.Tools;

namespace RaceManager.Cars
{
    [Serializable]
    [CreateAssetMenu(menuName = "Cars/CarVisualContainer", fileName = "CarVisualContainer", order = 1)]
    public class CarVisualContainer : SerializedScriptableObject
    {
        public DriverType DriverType;
        public CarName CarName;

        [FoldoutGroup("Materials Settings")]
        public MaterialSetType CurrentMaterialsSetType;

        [ReadOnly]
        [FoldoutGroup("Materials Settings")]
        public MaterialsContainer MaterialsContainer;

        [FoldoutGroup("Materials Settings")]
        [DictionaryDrawerSettings(KeyLabel = "Car", ValueLabel = "Sets")]
        public Dictionary<CarName, Dictionary<MaterialSetType, bool>> AvailableCarMaterials = new Dictionary<CarName, Dictionary<MaterialSetType, bool>>();

        public Material GetCurrentMaterial() => MaterialsContainer.GetMaterialTypeOf(CarName, CurrentMaterialsSetType);

        public void SetMaterialsContainer(MaterialsContainer container)
        {
            foreach (var pair in AvailableCarMaterials)
            {
                var holdersList = container.GetHoldersFor(pair.Key);
                foreach (var valuesDic in pair.Value)
                {
                    var holder = holdersList.Find(matHolder => matHolder.PartsSetType == valuesDic.Key);
                    holder.isAvailable = valuesDic.Value;
                } 
            }

            MaterialsContainer = container;
        }
        //[SerializeField]
        //[FoldoutGroup("Materials Settings")]
        //[DictionaryDrawerSettings(KeyLabel = "Set Type", ValueLabel = "Materials")]
        //private Dictionary<PartsSetType, MaterialsHolder> Materials = new Dictionary<PartsSetType, MaterialsHolder>()
        //{
        //    { PartsSetType.Default, new MaterialsHolder() },
        //    { PartsSetType.Military, new MaterialsHolder() },
        //};
        //private Dictionary<PartsSetType, List<MaterialName>> Materials = new Dictionary<PartsSetType, List<MaterialName>>()
        //{
        //    { PartsSetType.Default, new List<MaterialName>() },
        //    { PartsSetType.Military, new List<MaterialName>() },
        //};
        //private Dictionary<PartsSetType, MeshRenderer> MaterialsHolders = new Dictionary<PartsSetType, MeshRenderer>()
        //{
        //    { PartsSetType.Default, new MeshRenderer() },
        //    { PartsSetType.Military, new MeshRenderer() },
        //};

        [FoldoutGroup("Wheels Settings")]
        public WheelsSetType CurrentWheelsSetType;

        [FoldoutGroup("Wheels Settings")]
        public PartLevel CurrentWheelsLevel;

        [FoldoutGroup("Suspention Settings")]
        public PartLevel CurrentSuspentionLevel;

        [FoldoutGroup("Bumpers Settings")]
        public PartLevel CurrentBumpersLevel;

        [FoldoutGroup("Body Kits Settings")]
        public PartLevel CurrentBodyKitsLevel;

        //public MaterialsHolder CurrentMaterials => Materials[CurrentMaterialsSetType];
        //[JsonIgnore]
        //public Material[] CurrentMaterials
        //{
        //    get
        //    {
        //        List<Material> materials = new List<Material>();
        //        List<MaterialName> list = Materials[CurrentMaterialsSetType];
        //        foreach (var name in list)
        //        {
        //            string path = string.Concat(ResourcePath.MaterialsPrefabsFolder, name.ToString());
        //            Material mat = ResourcesLoader.LoadObject<Material>(path);
        //            materials.Add(mat);

        //            $"Materials loded fron: {path}; Count: {materials.Count}; Driver: {DriverType}".Log();
        //        }

        //        return materials.ToArray();
        //    }
        //}

        //[Serializable]
        //public class MaterialsHolder
        //{
        //    public Material[] Materials = new Material[0];
        //}
    }
}
