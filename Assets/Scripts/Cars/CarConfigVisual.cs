using System;
using UnityEngine;
using System.Collections.Generic;

namespace RaceManager.Cars
{
    [Serializable]
    public class CarConfigVisual
	{
        [SerializeField] private CarName _carName;
        [SerializeField] private CarBody _carBody;
        [SerializeField] private List<WheelsSet> _wheelsSets;
        [SerializeField] private List<SuspentionSet> _suspentionSets;
        [SerializeField] private List<BumperSet> _bumperSets;
        [SerializeField] private List<BodyKitSet> _bodyKitSets;
        //[SerializeField] private List<ConfigsList> _partConfigs;  

        public CarBody CarBody => _carBody;
        public CarName CarName => _carName;

        //public PartsSet GetPartConfig(PartType partType, MaterialSetType partsSetType)
        //{
        //    List<PartsSet> partConfigList = _partConfigs.Find(t => t.PartType == partType).PartConfigs;

        //    if (partConfigList != null)
        //    {
        //        PartsSet partConfig = partConfigList.Find(s => s.PartsSetType == partsSetType);
        //        return partConfig;
        //    }
        //    else
        //        return null;
        //}

        //[Serializable]
        //public class ConfigsList
        //{
        //    public PartType PartType;
        //    public List<PartsSet> PartConfigs;
        //}
    }
}
