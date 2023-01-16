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
        [ShowInInspector, ReadOnly] private LayerMask _roadMask;

        private Ray _ray;

        public void SetWaypoint(float maxHeight, float heightAboveRoad, LayerMask roadMask)
        { 
            _maxHeight = maxHeight;
            _heightAboveRoad = heightAboveRoad;
            _roadMask = roadMask;
        }

        //public void UpdatePositionHeight()
        //{
        //    UpdatePositionHeight(_maxHeight, _heightAboveRoad, _roadMask);
        //}

        public void UpdatePositionHeight(float maxHeight, float heightAboveRoad, LayerMask roadMask)
        {
            SetWaypoint(maxHeight, heightAboveRoad, roadMask);

            Vector3 pos = transform.position;
            _ray = new Ray(pos, Vector3.down);

            if (Physics.Raycast(_ray, out RaycastHit hit, _maxHeight))
            {
                pos = hit.point;
                pos.y += _heightAboveRoad;
                transform.position = pos;
                //Debug.Log($"[{name}] => HITS {hit.transform.name} => yPOS {pos.y} ");
            }
        }
    }
}
