using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Effects
{
    public class GroundDetection : MonoBehaviour
    {
        [SerializeField] private GroundConfig _defaultGroundConfig;

        private Dictionary<GameObject, IGround> _groundsDic = new Dictionary<GameObject, IGround>();

        public GroundConfig DefaultGroundConfig => _defaultGroundConfig;

        public bool TryGetGroundEntity(GameObject go, out IGround entity)
        {
            if (!_groundsDic.TryGetValue(go, out entity))
            {
                if (!go.TryGetComponent(out entity))
                {
                    return false;
                }

                _groundsDic.Add(go, entity);
                return true;
            }

            return true; 
        }
    }
}
