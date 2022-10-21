using Cinemachine;
using UnityEngine;

namespace RaceManager.Cameras
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class AdditionalCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _camera;

        public CinemachineVirtualCamera Camera => GetComponent<CinemachineVirtualCamera>();
    }
}
