using Cinemachine;
using UnityEngine;

namespace RaceManager.Cameras
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineTargetGroup _targetGroup;
        private CinemachineVirtualCamera _camera;

        public CinemachineVirtualCamera Camera
        {
            get
            { 
                if(_camera == null)
                    _camera = GetComponent<CinemachineVirtualCamera>();
                return _camera;
            }
        }

        public CinemachineTargetGroup TargetGroup => _targetGroup;

        public void SetTargetGroup()
        {
            if (_camera == null)
                _camera = GetComponent<CinemachineVirtualCamera>();
            _camera.LookAt = _targetGroup.Transform;
            _camera.Follow = _targetGroup.Transform;
        }

        public void SetTarget(Transform target)
        {
            _camera.LookAt = target;
            _camera.Follow = target;
        }
    }
}
