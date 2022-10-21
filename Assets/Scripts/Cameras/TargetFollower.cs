using UnityEngine;

namespace RaceManager.Cameras
{
    public class TargetFollower : MonoBehaviour
    { 
        private Transform _targetToFollow;

        private float _moveSpeed = 10f;
        private Vector3 _position;

        private bool _isFollowing;

        public void SetTarget(Transform target)
        { 
            _targetToFollow = target;
            _isFollowing = true;
        }

        private void Update()
        {
            _position = Vector3.Lerp(transform.position,  _targetToFollow.position, _moveSpeed * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!_isFollowing)
                return;

            transform.Translate(_position, Space.World);
        }

        private void OnDrawGizmos()
        {
            Color color = Gizmos.color;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.5f);
            Gizmos.color = color;
        }
    }
}
