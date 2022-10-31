using Cinemachine;
using UnityEngine;

namespace RaceManager.Cameras
{
    public class MenuCamerasHandler : MonoBehaviour
    {
        private const int MajorPriority = 10;
        private const int MinorPriority = 0;

        [SerializeField] private CinemachineVirtualCamera _closeCamera;
        [SerializeField] private CinemachineVirtualCamera _farCamera;

        private void Start()
        {
            _closeCamera.Priority = MajorPriority;
            _farCamera.Priority = MinorPriority;
        }

        public void LookAt(Transform lookAtTransform)
        {
            _closeCamera.LookAt = lookAtTransform;
            _farCamera.LookAt = lookAtTransform;
        }

        public void ToggleCamPriorities()
        {
            _closeCamera.Priority = _closeCamera.Priority == MajorPriority ? MinorPriority : MajorPriority;
            _farCamera.Priority = _farCamera.Priority == MajorPriority ? MinorPriority : MajorPriority;
        }
    }
}
