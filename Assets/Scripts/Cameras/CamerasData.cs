using UnityEngine;

namespace RaceManager.Cameras
{
    public struct CamerasData
    {
        public Transform cameraLookTarget;
        public Transform cameraFollowTarget;

        public Transform cameraFinalTarget;
        public Transform cameraFinalPosition;

        public Transform startCameraTarget;
        public Transform startCameraPosition;
    }
}
