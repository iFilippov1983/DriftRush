using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarBody : MonoBehaviour
    {
        public GameObject bumperFront;
        public GameObject bumperBack;
        public GameObject spoiler;

        [SerializeField] private Material[] _materials;
        [SerializeField] private List<MeshRenderer> _allMeshRenderers;

        private MeshRenderer mr_bumperFront;
        private MeshRenderer mr_bumperBack;
        private MeshRenderer mr_spoiler;

        //private void Awake()
        //{
        //    mr_bumperFront = bumperFront.GetComponent<MeshRenderer>();
        //    mr_bumperBack = bumperBack.GetComponent<MeshRenderer>();
        //    mr_spoiler = spoiler.GetComponent<MeshRenderer>();
        //}

        public void SetMaterials(params Material[] materials)
        { 
            _materials = materials;

            mr_bumperBack.materials = _materials;
            mr_bumperFront.materials = _materials;
            mr_spoiler.materials = _materials;

            foreach (var m in _allMeshRenderers)
                m.materials = _materials;
        }
    }
}
