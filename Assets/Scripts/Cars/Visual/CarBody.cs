using System;
using System.Collections.Generic;
using UnityEngine;
using static RaceManager.Cars.CarVisualContainer;

namespace RaceManager.Cars
{
    [Serializable]
    public class CarBody : MonoBehaviour
    {
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private List<MeshRenderer> _allMeshRenderers;

        public void SetMaterial(Material material)
        {
            if (material == null)
                SetToDefaultMaterial();

            foreach (var m in _allMeshRenderers)
                m.material = material;
        }

        public void SetToDefaultMaterial()
        {
            SetMaterial(_defaultMaterial);
        }
    }
}
