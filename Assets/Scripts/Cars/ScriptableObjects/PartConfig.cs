using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public class PartConfig
	{
        [SerializeField] 
        private PartsSetType _partsSetType;

        [SerializeField]
		private List<PartsList> _parts;

        private PartLevel _currentPartsLevel;

        public bool isAvailable;
        public PartsSetType PartsSetType => _partsSetType;

        public PartProperty CurrentProperties
        {
            get
            {
                var parts = GetPartsWhithLevel(_currentPartsLevel);
                return parts[0].Property;
            }
        }

        public void SetActive(PartLevel partLevel)
		{ 
			foreach(var list in _parts)
				if(list.PartLevel != partLevel)
					ToggleActivity(list.PartLevel, false);
			
			ToggleActivity(partLevel, true);
		}

        private List<Part> GetPartsWhithLevel(PartLevel partLevel)
        {
            PartsList partsList = _parts.Find(l => l.PartLevel == partLevel);
            return partsList.Parts;
        }

        private void ToggleActivity(PartLevel partLevel, bool active)
		{
            var list = _parts.Find(l => l.PartLevel == partLevel);
            foreach (var part in list.Parts)
            {
                if (part.Object != null)
                {
                    part.isActive = active;
                    part.Object.transform.localScale = part.Property.Scale;
                }
            }

            if (active)
                _currentPartsLevel = partLevel;
        }

        [Serializable]
        public class PartsList
        {
            public PartLevel PartLevel;
            public List<Part> Parts;
        }
    }
}