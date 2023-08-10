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
        //[SerializeField] private float _checkDistanceFactor = 2f;

        //private float _distanceFromStart;
        [ShowInInspector, ReadOnly]
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

        //[ShowInInspector, ReadOnly]
        //private IEnumerator _currentFadeJob;
        //[ShowInInspector, ReadOnly]
        //private IEnumerator _currentColorJob;

        [ShowInInspector, ReadOnly]
        private Task<bool> _currentFadeTask;
        [ShowInInspector, ReadOnly]
        private Task<bool> _currentColorTask;

        private CancellationTokenSource _cancelTokenFade = new CancellationTokenSource();
        private CancellationTokenSource _cancelTokenColor = new CancellationTokenSource();

        #region Minor variables

        private Transform _itsTransform;

        private Color m_ColorToFade;

        private Color m_ColorToChange;
        private Color m_TargetColor;

        #endregion

        private Material MeshMaterial { get; set; }

        public Subject<(bool isVisible, RaceLineSegment segment)> OnVisibilityChange = new Subject<(bool isVisible, RaceLineSegment segment)>();

        [ShowInInspector, ReadOnly]
        public float DistanceFromStart { get; set; }

        [ShowInInspector, ReadOnly]
        public int CurrentIndex { get; set; }

        public Transform Transform => _itsTransform;

        public void Initiallize(RaceLineSegmentData data)
        {
            _itsTransform = transform;

            _recomendedSpeed = data.recomendedSpeed;
            _fadeSpeed = data.fadeSpeed;
            _colorTransitionSpeed = data.colorTransitionSpeed;
            _baseColor = data.baseColor;
            _warningColor = data.warningColor;
            _checkOffset = data.checkOffset;

            _isVisible = true;
            _isWarning = false;

            MeshMaterial = _mesh.material;
            MeshMaterial.color = _baseColor;
        }

        #region Using Tasks implementation

        public async void DistanceCheck(float distance)
        {
            _checkDistance = distance + _checkOffset;
            bool isPassed = _checkDistance >= DistanceFromStart;

            if (isPassed == _isVisible && (_currentFadeTask == null || _currentFadeTask.IsCompleted))
            {
                _currentFadeTask = VisibilityChange(isPassed, _cancelTokenFade.Token);
                await _currentFadeTask;

                while (!_currentFadeTask.IsCompleted)
                {
                    _cancelTokenFade.Token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                _isVisible = _currentFadeTask.Result;
                OnVisibilityChange?.OnNext((isVisible: _isVisible, segment: this));
            }
        }

        public async void SpeedCheck(float speed)
        {
            if (!_isVisible) return;

            bool isOverspeed = speed >= _recomendedSpeed;

            if (isOverspeed != _isWarning && (_currentColorTask == null || _currentColorTask.IsCompleted))
            {
                _currentColorTask = ColorChange(isOverspeed, _cancelTokenColor.Token);
                await _currentColorTask;

                while (!_currentColorTask.IsCompleted)
                {
                    _cancelTokenColor.Token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                _isWarning = _currentColorTask.Result;
            }
        }

        private async Task<bool> VisibilityChange(bool fade, CancellationToken token)
        {
            if (MeshMaterial == null) return false;

            m_ColorToFade = MeshMaterial.color;

            float maxAlphaValue = _isWarning
                ? _warningColor.a
                : _baseColor.a;

            float targetAlpha = fade ? 0 : maxAlphaValue;

            while (!Mathf.Approximately(m_ColorToFade.a, targetAlpha))
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    m_ColorToFade.a = Mathf.Lerp(m_ColorToFade.a, targetAlpha, Time.deltaTime * _fadeSpeed);
                    MeshMaterial.color = m_ColorToFade;

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

            m_ColorToChange = MeshMaterial.color;
            m_TargetColor = warning ? _warningColor : _baseColor;
            m_TargetColor.a = MeshMaterial.color.a;

            while (!Equals(m_ColorToChange, m_TargetColor))
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    m_ColorToChange = Color.Lerp(m_ColorToChange, m_TargetColor, Time.deltaTime * _colorTransitionSpeed);
                    m_ColorToChange.a = MeshMaterial.color.a;
                    m_TargetColor.a = MeshMaterial.color.a;
                    MeshMaterial.color = m_ColorToChange;

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
        }
    }
}
