using Cinemachine;
using RaceManager.Race;
using RaceManager.Waypoints;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Cameras
{
    public class RaceCamerasHandler : MonoBehaviour
    {
        private const int MajorPriority = 10;
        private const int MinorPriority = 0;

        [SerializeField] private WaypointTrack _pointsTrack;
        [SerializeField] private FollowCamera _followGroupCamera;
        [SerializeField] private CinemachineVirtualCamera _followCamera;
        [SerializeField] private CinemachineVirtualCamera _finishCamera;
        [SerializeField] private CinemachineVirtualCamera _startCamera;
        [SerializeField] private CinemachineSmoothPath _path;
        [SerializeField] private CinemachineDollyCart _cart;

        public void InitializeCamerasWhith(Transform carTransform, Transform followTarget)
        {
            //_followGroupCamera.TargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
            //_followGroupCamera.TargetGroup.AddMember(carTransform, 1f, 0f);
            //_followGroupCamera.TargetGroup.AddMember(followTarget, 1f, 0f);
            //_followGroupCamera.SetTargetGroup();
            _followCamera.LookAt = followTarget;
            _followCamera.Follow = followTarget;

            _startCamera.LookAt = carTransform;
            _finishCamera.LookAt = carTransform;

            RaceEventsHub.Subscribe(RaceEventType.COUNTDOWN, SetStartCamera);
            RaceEventsHub.Subscribe(RaceEventType.START, SetFollowCamera);
            RaceEventsHub.Subscribe(RaceEventType.FINISH, SetFinishCamera);
        }

        private void SetFinishCamera()
        {
            _finishCamera.Priority = MajorPriority;

            _startCamera.Priority = MinorPriority;
            _followGroupCamera.Camera.Priority = MinorPriority;
            _followCamera.Priority = MinorPriority;
        }

        private void SetFollowCamera()
        {
            //_followCamera.Priority = MajorPriority;
            _followGroupCamera.Camera.Priority = MajorPriority;

            _finishCamera.Priority = MinorPriority;
            _startCamera.Priority = MinorPriority;
        }

        private void SetStartCamera()
        {
            _startCamera.Priority = MajorPriority;

            _followCamera.Priority = MinorPriority;
            _followGroupCamera.Camera.Priority = MinorPriority;
            _finishCamera.Priority = MinorPriority;
        }

        [Button]
        private void MakePath()
        {
            _path.m_Waypoints = new CinemachineSmoothPath.Waypoint[_pointsTrack.Waypoints.Length];
            for (int i = 0; i < _pointsTrack.Waypoints.Length; i++)
            { 
                _path.m_Waypoints[i].position = _pointsTrack.waypointList.items[i].position;
            }
        }

        private void OnDestroy()
        {
            RaceEventsHub.Unsunscribe(RaceEventType.COUNTDOWN, SetStartCamera);
            RaceEventsHub.Unsunscribe(RaceEventType.START, SetFollowCamera);
            RaceEventsHub.Unsunscribe(RaceEventType.FINISH, SetFinishCamera);
        }
    }
}
