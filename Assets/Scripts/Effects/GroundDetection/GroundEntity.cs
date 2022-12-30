using UnityEngine;

namespace RaceManager.Effects
{
    public class GroundEntity : MonoBehaviour, IGround
    {
        [SerializeField] private GroundConfig _config;

        private void OnEnable()
        {
            _config.SetGroundLayer(this);
        }

        public LayerMask LayerMask => gameObject.layer;
        public GroundConfig Config => _config;
    }
}
