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
        [SerializeField] private List<ConfigsList> _partConfigs;  

        public CarBody CarBody => _carBody;
        public CarName CarName => _carName;

        public PartConfig GetPartConfig(PartType partType, PartsSetType partsSetType)
        {
            List<PartConfig> partConfigList = _partConfigs.Find(t => t.PartType == partType).PartConfigs;

            if (partConfigList != null)
            {
                PartConfig partConfig = partConfigList.Find(s => s.PartsSetType == partsSetType);
                return partConfig;
            }
            else
                return null;
        }

        [Serializable]
        public class ConfigsList
        {
            public PartType PartType;
            public List<PartConfig> PartConfigs;
        }
    }
}
