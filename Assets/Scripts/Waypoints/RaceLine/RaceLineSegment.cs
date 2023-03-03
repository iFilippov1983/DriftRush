using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class RaceLineSegment : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _mesh;
        [SerializeField] private float _checkDistanceFactor = 2f;

        private float _distanceFromStart;
        private float _recomendedSpeed;
        private float _fadeSpeed;
        private float _colorTransitionSpeed;
        private float _checkOffset;

        [ShowInInspector, ReadOnly]
        private float _checkDistance;

        private Color _baseColor;
        private Color _warningColor;

        [ShowInInspector, ReadOnly]
        private bool _isVisible = true;
        [ShowInInspector, ReadOnly]
        private bool _isWarning = false;

        [ShowInInspector, ReadOnly]
        private IEnumerator _currentFadeJob;
        [ShowInInspector, ReadOnly]
        private IEnumerator _currentColorJob;

        private Task<bool> _currentFadeTask;
        private Task<bool> _currentColorTask;
        private CancellationTokenSource _cancelTokenFade = new CancellationTokenSource();
        private CancellationTokenSource _cancelTokenColor = new CancellationTokenSource();

        private Material MeshMaterial => _mesh.material;

        public Action<RaceLineSegment> OnDestroyAction;

        [ShowInInspector, ReadOnly]
        public float DistanceFromStart
        { 
            get => _distanceFromStart; 
            set { _distanceFromStart = value; } 
        }

        public void Initiallize(RaceLineSegmentData data)
        {
            _recomendedSpeed = data.recomendedSpeed;
            _fadeSpeed = data.fadeSpeed;
            _colorTransitionSpeed = data.colorTransitionSpeed;
            _baseColor = data.baseColor;
            _warningColor = data.warningColor;
            _checkOffset = data.checkOffset;

            MeshMaterial.color = _baseColor;
        }

        #region Using Tasks implementation

        public async void DistanceCheck(float distance)
        {
            _checkDistance = distance + _checkOffset;
            bool isPassed = _checkDistance >= _distanceFromStart;

            if (isPassed == _isVisible && (_currentFadeTask == null || _currentFadeTask.IsCompleted))
            {
                _currentFadeTask = VisibilityChange(isPassed, _cancelTokenFade.Token);
                await _currentFadeTask;
                _isVisible = _currentFadeTask.Result;
            }
        }

        public async void SpeedCheck(float speed)
        {
            if (!_isVisible) return;

            bool isInRange = _checkDistance * _checkDistanceFactor >= _distanceFromStart;
            bool isOverspeed = speed >= _recomendedSpeed;

            if (isOverspeed != _isWarning && isInRange && (_currentColorTask == null || _currentColorTask.IsCompleted))
            {
                _currentColorTask = ColorChange(isOverspeed, _cancelTokenColor.Token);
                await _currentColorTask;
                _isWarning = _currentColorTask.Result;
            }
        }

        private async Task<bool> VisibilityChange(bool fade, CancellationToken token)
        {
            if (MeshMaterial == null) return false;

            Color color = MeshMaterial.color;

            float maxAlphaValue = _isWarning
                ? _warningColor.a
                : _baseColor.a;

            float targetAlpha = fade ? 0 : maxAlphaValue;

            while (!Mathf.Approximately(color.a, targetAlpha))
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * _fadeSpeed);
                    MeshMaterial.color = color;

                    await Task.Yield();
                }
                catch (Exception)
                {
                    return !fade;
                }
                
            }

            return !fade;
        }

        private async Task<bool> ColorChange(bool warning, CancellationToken token)
        {
            if (MeshMaterial == null) return false;

            Color color = MeshMaterial.color;
            Color targetColor = warning ? _warningColor : _baseColor;
            targetColor.a = MeshMaterial.color.a;

            while (!Equals(color, targetColor))
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    color = Color.Lerp(color, targetColor, Time.deltaTime * _colorTransitionSpeed);
                    color.a = MeshMaterial.color.a;
                    targetColor.a = MeshMaterial.color.a;
                    MeshMaterial.color = color;

                    await Task.Yield();
                }
                catch (Exception)
                {
                    return warning;
                }
            }

            return warning;
        }
        #endregion

        #region Using Coroutines implementation 

        //public void CheckDistance(float distance)
        //{
        //    _checkDistance = distance + _checkOffset;
        //    bool isPassed = _checkDistance > _distanceFromStart;

        //    if (isPassed == _isVisible && _currentFadeJob == null)
        //    {
        //        //if (_currentFadeJob != null && !isPassed)
        //        //    StopCoroutine(_currentFadeJob);

        //        _currentFadeJob = ChangeVisibility(isPassed);
        //        StartCoroutine(_currentFadeJob);
        //    }
        //}

        //public void CheckSpeed(float speed)
        //{
        //    if (!_isVisible) return;

        //    bool isInRange = _checkDistance * _checkDistanceFactor > _distanceFromStart;
        //    bool isOverspeed = speed >= _recomendedSpeed;

        //    if (isOverspeed != _isWarning && _currentColorTask == null && isInRange)
        //    {
        //        //if (_currentColorJob != null)
        //        //    StopCoroutine(_currentColorJob);

        //        _currentColorJob = ChangeColor(isOverspeed);
        //        StartCoroutine(_currentColorJob);
        //    }
        //}

        //private IEnumerator ChangeVisibility(bool fade)
        //{
        //    Color color = MeshMaterial.color;

        //    float maxAlphaValue = _isWarning
        //        ? _warningColor.a
        //        : _baseColor.a;

        //    float targetAlpha = fade ? 0 : maxAlphaValue;

        //    while (!Mathf.Approximately(color.a, targetAlpha))
        //    {
        //        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * _fadeSpeed);
        //        MeshMaterial.color = color;

        //        yield return null;
        //    }

        //    _isVisible = !fade;
        //    _currentFadeJob = null;
        //}

        //private IEnumerator ChangeColor(bool warning)
        //{
        //    Color color = MeshMaterial.color;
        //    Color targetColor = warning ? _warningColor : _baseColor;
        //    targetColor.a = color.a;

        //    while (!Equals(color, targetColor))
        //    {
        //        color = Color.Lerp(color, targetColor, Time.deltaTime * _colorTransitionSpeed);
        //        MeshMaterial.color = color;

        //        yield return null;
        //    }

        //    _isWarning = warning;
        //    _currentColorTask = null;
        //}

        #endregion

        private void OnDestroy()
        {
            _cancelTokenFade.Cancel();
            _cancelTokenColor.Cancel();

            StopAllCoroutines();

            OnDestroyAction?.Invoke(this);
        }
    }
}
