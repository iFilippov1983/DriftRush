using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public class CarBody : MonoBehaviour
    {
        [SerializeField] private Material[] _defaultMaterials;
        [SerializeField] private List<MeshRenderer> _allMeshRenderers;

        public void SetMaterials(params Material[] materials)
        {
            foreach (var m in _allMeshRenderers)
                m.materials = materials;
        }

        public void SetToDefaultMaterials()
        {
            SetMaterials(_defaultMaterials);
        }
    }
}
