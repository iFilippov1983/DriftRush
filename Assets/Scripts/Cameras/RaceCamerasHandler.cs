using Cinemachine;
using RaceManager.Race;
using RaceManager.Root;
using System.Collections;
using UnityEngine;

namespace RaceManager.Cameras
{
    public class RaceCamerasHandler : MonoBehaviour
    {
        private const int MajorPriority = 10;
        private const int MinorPriority = 0;

        private const float DefaultBlendTime = 0.5f;
        private const float CountdownBlendTime = 3f;

        [Header("Speed effect settings")]
        [Tooltip("If TRUE, values change for speed effects will depend on Car current speed")]
        [SerializeField] private bool _useSpeedFactor = true;
        [Range(0f, 2f)]
        [SerializeField] private float _cameraShakeAmountForSpeed = 1f;
        [Range(1f, 10f)]
        [SerializeField] private float _shakeSpeedForSpeed = 1f;
        [Range(0f, 10f)]
        [SerializeField] private float _xDampingChangeSpeed = 5f;
        [Range(1f, 10f)]
        [SerializeField] private float _fovEncreaseSpeed = 1f;
        [Range(1f, 10f)]
        [SerializeField] private float _fovDecreaseSpeed = 2f;
        [SerializeField] private float _maxExtraFovValue = 5f;
        [Space]
        [Header("Off road effect settings")]
        [Range(0f, 2f)]
        [SerializeField] private float _cameraShakeAmountForOffRoad = 1f;
        [Range(0f, 10f)]
        [SerializeField] private float _shakeSpeedForOffRoad = 2f;
        [Space]
        [SerializeField] private CinemachineBrain _cinemachineBrain;
        [SerializeField] private CinemachineVirtualCamera _followCamera;
        [SerializeField] private CinemachineVirtualCamera _finishCamera;
        [SerializeField] private CinemachineVirtualCamera _startCamera;

        private IEnumerator _currentShakeJob;
        private IEnumerator _currentFovJob;
        private IEnumerator _currentDampingJob;

        private CinemachineTransposer _transposer;
        private CinemachineComposer _composer;
        private Transform _cameraFinalTarget;
        private Transform _cameraFinalPosition;
        private float _defaultMainCamFov;
        private float _defaultMainCamDampingX;
        private Vector3 _defaultCamAimOffset;

        private float CurrentCamFov => _followCamera.m_Lens.FieldOfView;
        private float CurrentCamDampingX
        { 
            get => _transposer.m_XDamping;
            set { _transposer.m_XDamping = value; }
        }
        private Vector3 CameraAimOffset
        {
            get => _composer.m_TrackedObjectOffset;
            set { _composer.m_TrackedObjectOffset = value; }
        } 

        public Transform FollowCam => _followCamera.transform;
        public Transform FinishCam => _finishCamera.transform;
        public Transform StartCam => _startCamera.transform;


        #region Public Functions

        public void SetTargets(CamerasData data)
        {
            _followCamera.LookAt = data.cameraLookTarget; 
            _followCamera.Follow = data.cameraFollowTarget;

            _startCamera.LookAt = data.startCameraTarget; 
            _finishCamera.LookAt = data.cameraFollowTarget;

            _transposer = _followCamera.GetCinemachineComponent<CinemachineTransposer>();
            _composer = _followCamera.GetCinemachineComponent<CinemachineComposer>();
            _defaultMainCamDampingX = CurrentCamDampingX;
            _defaultMainCamFov = CurrentCamFov;
            _defaultCamAimOffset = CameraAimOffset;

            _cameraFinalTarget = data.cameraFinalTarget;
            _cameraFinalPosition = data.cameraFinalPosition;

            SetStartCamera();

            EventsHub<RaceEvent>.Subscribe(RaceEvent.COUNTDOWN, SetFollowCamera);
            EventsHub<RaceEvent>.Subscribe(RaceEvent.FINISH, SetFinishCamera);
        }

        public void InvokeSpeedEffect(float curSpeed, float maxSpeed, bool doShake)
        {
            float speedFactor = _useSpeedFactor
                ? curSpeed / maxSpeed
                : 1f;

            if (_currentFovJob != null)
                StopCoroutine(_currentFovJob);

            float newFovValue = _defaultMainCamFov + _maxExtraFovValue * speedFactor;
            bool isBraking = CurrentCamFov > newFovValue;
            float fovChangeSpeed = isBraking ? _fovDecreaseSpeed : _fovEncreaseSpeed;
            _currentFovJob = ChangeCameraFov(_followCamera, CurrentCamFov, newFovValue, fovChangeSpeed);
            StartCoroutine(_currentFovJob);

            if (_currentDampingJob != null)
                StopCoroutine(_currentDampingJob);

            float newDampingValue = _defaultMainCamDampingX - _defaultMainCamDampingX * speedFactor;
            float dampingChangeSpeed = _xDampingChangeSpeed * speedFactor;
            _currentDampingJob = ChangeCameraFollowDamping(CurrentCamDampingX, newDampingValue, dampingChangeSpeed);
            StartCoroutine(_currentDampingJob);

            float shakeAmount = _cameraShakeAmountForSpeed * speedFactor;
            float shakeSpeed = _shakeSpeedForSpeed * speedFactor;
            InvokeCameraShakeEffect(shakeAmount, shakeSpeed, doShake);
        }

        public void StopSpeedEffect()
        {
            ChangeCameraFovToDefault();
            ChangeCameraFollowDampingToDefault();
            StopCameraShakeEffect();
        }

        public void InvokeCameraShakeEffect()
        {
            InvokeCameraShakeEffect(_cameraShakeAmountForOffRoad, _shakeSpeedForOffRoad, true);
        }

        public void StopCameraShakeEffect()
        {
            if (_currentShakeJob == null)
                return;

            StopCoroutine(_currentShakeJob);
            _currentShakeJob = null;
            CameraAimOffset = _defaultCamAimOffset;
        }

        public void SetCameraToFinalPosition()
        {
            _finishCamera.transform.position = _cameraFinalPosition.position;
            _finishCamera.LookAt = _cameraFinalTarget;
        }

        #endregion

        #region Private Functions

        private void InvokeCameraShakeEffect(float shakeAmount, float shakeSpeed, bool doShake)
        {
            if (_currentShakeJob != null)
                StopCoroutine(_currentShakeJob);

            _currentShakeJob = ShakeCamera(shakeAmount, shakeSpeed, doShake);
            StartCoroutine(_currentShakeJob);

            //Debug.Log("[SHAKING]");
        }

        private IEnumerator ChangeCameraFov(CinemachineVirtualCamera cmCamera, float fromValue, float toValue, float changeSpeed)
        {
            while (!Mathf.Approximately(fromValue, toValue))
            {
                fromValue = Mathf.Lerp(fromValue, toValue, Time.deltaTime * changeSpeed);
                cmCamera.m_Lens.FieldOfView = fromValue;
                yield return null;
            }
        }

        private void ChangeCameraFovToDefault()
        {
            if (_currentFovJob == null)
                return;

            StopCoroutine(_currentFovJob);

            _currentFovJob = Mathf.Approximately(CurrentCamFov, _defaultMainCamFov)
                ? null
                : ChangeCameraFov(_followCamera, CurrentCamFov, _defaultMainCamFov, _fovDecreaseSpeed);

            if(_currentFovJob != null)
                StartCoroutine(_currentFovJob);
        }

        private IEnumerator ChangeCameraFollowDamping(float fromValue, float toValue, float changeSpeed)
        {
            while (!Mathf.Approximately(fromValue, toValue))
            {
                fromValue = Mathf.Lerp(fromValue, toValue, Time.deltaTime * changeSpeed);
                CurrentCamDampingX = fromValue;
                yield return null;
            }
        }

        private void ChangeCameraFollowDampingToDefault()
        {
            if (_currentDampingJob == null)
                return;

            StopCoroutine(_currentDampingJob);
            _currentDampingJob = null;
            CurrentCamDampingX = _defaultMainCamDampingX;
        }

        private IEnumerator ShakeCamera(float shakeAmount, float shakeSpeed, bool shake)
        {
            while (shake)
            {
                Vector3 randomPoint = _defaultCamAimOffset + Random.insideUnitSphere * shakeAmount;
                randomPoint.z = _defaultCamAimOffset.z;
                CameraAimOffset = Vector3.Lerp(CameraAimOffset, randomPoint, Time.deltaTime * shakeSpeed);
                yield return null;
            }

            CameraAimOffset = _defaultCamAimOffset;
        }

        private void SetFinishCamera()
        {
            _cinemachineBrain.m_DefaultBlend.m_Time = DefaultBlendTime;
            SetCameraState(MajorPriority, true, _finishCamera);
            SetCameraState(MinorPriority, false, _startCamera, _followCamera);
        }

        private void SetFollowCamera()
        {
            _cinemachineBrain.m_DefaultBlend.m_Time = CountdownBlendTime;
            SetCameraState(MajorPriority, true, _followCamera);
            SetCameraState(MinorPriority, false, _finishCamera, _startCamera);
        }

        private void SetStartCamera()
        {
            _cinemachineBrain.m_DefaultBlend.m_Time = DefaultBlendTime;
            SetCameraState(MajorPriority, true, _startCamera);
            SetCameraState(MinorPriority, false, _followCamera, _finishCamera);
        }

        private void SetCameraState(int priority, bool isActive, params CinemachineVirtualCamera[] cameras)
        {
            foreach (var cam in cameras)
            {
                cam.Priority = priority;
            }
        }

        private void OnDestroy()
        {
            EventsHub<RaceEvent>.Unsunscribe(RaceEvent.COUNTDOWN, SetStartCamera);
            EventsHub<RaceEvent>.Unsunscribe(RaceEvent.FINISH, SetFinishCamera);
        }

        #endregion
    }
}
