using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class TrackNode : MonoBehaviour 
    {
        public float recomendedSpeed;
        public bool isCheckpoint;

        [ShowInInspector, ReadOnly] private float _maxHeight;
        [ShowInInspector, ReadOnly] private float _heightAboveRoad;

        private Ray _ray;

        public void SetWaypoint(float maxHeight, float heightAboveRoad)
        { 
            _maxHeight = maxHeight;
            _heightAboveRoad = heightAboveRoad;
        }

        public void UpdatePositionHeight(float maxHeight, float heightAboveRoad)
        {
            SetWaypoint(maxHeight, heightAboveRoad);

            Vector3 pos = transform.position;
            _ray = new Ray(pos, Vector3.down);

            if (Physics.Raycast(_ray, out RaycastHit hit, _maxHeight))
            {
                pos = hit.point;
                pos.y += _heightAboveRoad;
                transform.position = pos;
            }
        }
    }
}
