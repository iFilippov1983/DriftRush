using UnityEngine;

namespace RaceManager.Effects
{
    public class GroundEntity : MonoBehaviour, IGround
    {
        [SerializeField] private GroundConfig _config;

        public GroundConfig Config => _config;
    }
}
