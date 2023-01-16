using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class RaceLineSegment : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _mesh;

        private float _distanceFromStart;
        private float _recomendedSpeed;

        public MeshRenderer Mesh => _mesh;

        [ShowInInspector, ReadOnly]
        public float DistanceFromStart
        { 
            get => _distanceFromStart; 
            set { _distanceFromStart = value; } 
        }

        [ShowInInspector, ReadOnly]
        public float RecomendedSpeed
        { 
            get => _recomendedSpeed;
            set { _recomendedSpeed = value; }
        }

        public bool IsPassed(float disance) => disance > _distanceFromStart;
        public bool IsInRange(float value) => _distanceFromStart <= value;
    }
}
