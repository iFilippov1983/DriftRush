using Cinemachine;
using RaceManager.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cameras
{
    public class MenuCamerasHandler : SerializedMonoBehaviour
    {
        private const int MajorPriority = 10;
        private const int MinorPriority = 0;

        [SerializeField] private CinemachineVirtualCamera _closeCamera;
        [SerializeField] private CinemachineVirtualCamera _midCamera;
        [SerializeField] private CinemachineVirtualCamera _farCamera;

        [DictionaryDrawerSettings(KeyLabel = "Status", ValueLabel = "Camera")]
        [SerializeField] private Dictionary<MainUIStatus, CinemachineVirtualCamera> _camScheme = new Dictionary<MainUIStatus, CinemachineVirtualCamera>();

        public void LookAt(Transform lookAtTransform)
        {
            _closeCamera.LookAt = lookAtTransform;
            _midCamera.LookAt = lookAtTransform;
            _farCamera.LookAt = lookAtTransform;
        }

        public void ToggleCamPriorities(MainUIStatus status)
        {
            foreach (var cam in _camScheme.Values)
            {
                cam.Priority = MinorPriority;
            }

            var majorCam = _camScheme[status];
            if(majorCam != null)
                majorCam.Priority = MajorPriority;

            //Debug.Log($"Status [{status}] => Close cam: {_closeCamera.Priority} | Mid cam: {_midCamera.Priority} | Far cam: {_farCamera.Priority}");
        }
    }
}
