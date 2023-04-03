using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public class CarBody : MonoBehaviour
    {
        private const float VisibilityCheckInterval = 0.5f;

        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private List<MeshRenderer> _allMeshRenderers;

        private float _lastCheckTime;
        private bool _isVisible;

        public bool IsVisible
        {
            get 
            {
                if (Time.time - _lastCheckTime > VisibilityCheckInterval)
                {
                    _isVisible = false;
                    foreach (var renderer in _allMeshRenderers)
                    {
                        if (renderer.isVisible)
                        { 
                            _isVisible = true;
                            break;
                        }
                    }
                    _lastCheckTime = Time.time;
                }
                return _isVisible;
            }
        }

        private void Awake()
        {
            foreach (var renderer in _allMeshRenderers)
            {
                renderer.OnDestroyAsObservable()
                    .Take(1)
                    .Subscribe(u => _allMeshRenderers.Remove(renderer));
            }
        }

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
