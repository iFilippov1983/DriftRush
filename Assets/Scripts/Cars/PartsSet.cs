using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public abstract class PartsSet<T> where T : IPart
	{
        [ShowInInspector, ReadOnly]
        protected PartLevel _currentPartsLevel;
        protected PartLevel _previousePartsLevel;
        public bool isAvailable;
        [SerializeField] protected List<PartsList<T>> _parts;


        public PartLevel CurrentPartsLevel => _currentPartsLevel;
        public List<PartsList<T>> AllParts() => _parts;

        public void SetPartsLevel(PartLevel partLevel)
        {
            if(partLevel == _currentPartsLevel)
                return;

            _previousePartsLevel = _currentPartsLevel;
            _currentPartsLevel = partLevel;

            var previouse = _parts.Find(l => l.PartLevel == _previousePartsLevel);
            previouse.Activate(false);

            var current = _parts.Find(l => l.PartLevel == _currentPartsLevel);
            current.Activate(true);
        }

        [Serializable]
        public class PartsList<P> where P : T
        {
            public PartLevel PartLevel;
            public List<P> Parts;

            public PartsList()
            {
                Parts = new List<P>();
            }

            public void Activate(bool isActive)
            {
                foreach (var part in Parts)
                {
                    if (part != null)
                        part.IsActive = isActive;
                }
            }
        }
    }

    [Serializable]
    public class WheelsSet : PartsSet<WheelPart>
    {
        [SerializeField] private WheelsSetType _wheelsSetType;
        [SerializeField] private MeshRenderer[] _wheelMeshes = new MeshRenderer[4];

        public WheelsSet()
        {
            _parts = new List<PartsList<WheelPart>>();
        }

        public WheelsSetType WheelsSetType => _wheelsSetType;

        public void Install() => ToggleActivity(true);
        public void UnInstall() => ToggleActivity(false);

        private void ToggleActivity(bool active)
        {
            for (int i = 0; i < _wheelMeshes.Length; i++)
                _wheelMeshes[i].SetActive(active);
        }
    }

    [Serializable]
    public class SuspentionSet : PartsSet<SuspentionPart>
    {
        public SuspentionSet()
        {
            _parts = new List<PartsList<SuspentionPart>>();
        }
    }

    [Serializable]
    public class BumperSet : PartsSet<BumperPart>
    {
        public BumperSet()
        {
            _parts = new List<PartsList<BumperPart>>();
        }
    }

    [Serializable]
    public class BodyKitSet : PartsSet<BodyKitPart>
    {
        public BodyKitSet()
        {
            _parts = new List<PartsList<BodyKitPart>>();
        }
    }
}