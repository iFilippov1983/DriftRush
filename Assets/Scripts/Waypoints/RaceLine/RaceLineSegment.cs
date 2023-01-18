using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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
        private Color _currentColor;

        [ShowInInspector, ReadOnly]
        private bool _isVisible = true;
        [ShowInInspector, ReadOnly]
        private bool _isWarning = false;
        private bool _isFading;

        [ShowInInspector, ReadOnly]
        private IEnumerator _currentFadeJob;
        [ShowInInspector, ReadOnly]
        private IEnumerator _currentColorJob;
        private IEnumerator _currentJob;

        private Transform _transform;

        private Material MeshMaterial => _mesh.material;

        public Action<bool> OnSpeedCheckAction;
        public Action<RaceLineSegment> OnDestroyAction;

        [ShowInInspector, ReadOnly]
        public float DistanceFromStart
        { 
            get => _distanceFromStart; 
            set { _distanceFromStart = value; } 
        }

        [ShowInInspector, ReadOnly]
        public float RecomendedSpeed => _recomendedSpeed;

        public void Initiallize(RaceLineSegmentData data)
        {
            _recomendedSpeed = data.recomendedSpeed;
            _fadeSpeed = data.fadeSpeed;
            _colorTransitionSpeed = data.colorTransitionSpeed;
            _baseColor = data.baseColor;
            _warningColor = data.warningColor;
            _checkOffset = data.checkOffset;

            _transform = transform;

            MeshMaterial.color = _baseColor;
        }

        public void CheckDistance(float distance)
        {
            _checkDistance = distance + _checkOffset;
            bool isPassed = _checkDistance > _distanceFromStart;

            if (isPassed == _isVisible)
            {
                if (_currentFadeJob != null)
                    StopCoroutine(_currentFadeJob);

                _isVisible = !isPassed;
                //Debug.Log($"Is visible: {_isVisible}");
                _currentFadeJob = ChangeVisibility(isPassed);
                StartCoroutine(_currentFadeJob);
            }
        }

        public void CheckSpeed(float speed)
        {
            if (!_isVisible)
                return;

            //bool isInRange = _checkDistance * _checkDistanceFactor > _distanceFromStart;
            bool isOverspeed = speed >= _recomendedSpeed;

            //OnSpeedCheckAction?.Invoke(isOverspeed);

            if (isOverspeed != _isWarning || !_isVisible)// && isInRange)
            {
                if(_currentColorJob != null)
                    StopCoroutine(_currentColorJob);

                _isWarning = isOverspeed;
                _currentColorJob = ChangeColor(isOverspeed);
                StartCoroutine(_currentColorJob);
            }
        }

        private IEnumerator ChangeVisibility(bool fade)
        {
            _currentColor = MeshMaterial.color;
            _isFading = fade;
            float targetAlpha = fade ? 0 : 1;

            while (!Mathf.Approximately(_currentColor.a, targetAlpha))
            {
                _currentColor.a = Mathf.Lerp(_currentColor.a, targetAlpha, Time.deltaTime * _fadeSpeed);
                MeshMaterial.color = _currentColor;

                yield return null;
            }
        }

        private IEnumerator ChangeColor(bool warning)
        { 
            _currentColor = MeshMaterial.color;
            Color targetColor = warning ? _warningColor : _baseColor;

            while (!Equals(_currentColor, targetColor))
            {
                _currentColor = Color.Lerp(_currentColor, targetColor, Time.deltaTime * _colorTransitionSpeed);
                MeshMaterial.color = _currentColor;

                yield return null;
            }
        }

        private void OnDestroy()
        {
            OnDestroyAction?.Invoke(this);
            StopAllCoroutines();
        }
    }
}
