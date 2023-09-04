using DG.Tweening;
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

        private float _checkOffset;

        private float _colorTransitionDuration;
        private float _alphaTransitionDuration;

        [ShowInInspector, ReadOnly]
        private float _checkDistance;

        private Color _baseColor;
        private Color _warningColor;

        [ShowInInspector, ReadOnly]
        private bool _isVisible = true;
        [ShowInInspector, ReadOnly]
        private bool _isWarning = false;

        private Tween _colorTransitionTween;
        private Tween _alphaTransitionTween;

        #region Minor variables

        private Transform _itsTransform;

        private (bool isVisible, RaceLineSegment segment) _infoTuple;

        private Color m_TargetColor;

        #endregion

        [ReadOnly]
        public float RecomendedSpeed { get; set; }

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

            RecomendedSpeed = data.recomendedSpeed;

            _colorTransitionDuration = data.colorTransitionDuration;
            _alphaTransitionDuration = data.alphaTransitionDuration;

            _baseColor = data.baseColor;
            _warningColor = data.warningColor;
            _checkOffset = data.checkOffset;

            _isVisible = true;
            _isWarning = false;

            MeshMaterial = _mesh.material;
            MeshMaterial.color = _baseColor;
        }

        #region Using Tasks implementation

        public void DistanceCheck(float distance)
        {
            _checkDistance = distance + _checkOffset;
            bool isPassed = _checkDistance >= DistanceFromStart;

            if (isPassed == _isVisible && !_alphaTransitionTween.IsActive())
            {
                VisibilityChange(isPassed);
            }
        }

        public void SpeedCheck(float speed)
        {
            if (!_isVisible) return;

            bool isOverspeed = speed >= RecomendedSpeed;

            if (isOverspeed != _isWarning && !_colorTransitionTween.IsActive())
            {
                ColorChange(isOverspeed);
            }
        }

        private void VisibilityChange(bool fade)
        {
            if (MeshMaterial == null) return;

            float maxAlphaValue = _isWarning
                ? _warningColor.a
                : _baseColor.a;

            float targetAlpha = fade ? 0 : maxAlphaValue;

            if (_colorTransitionTween.IsActive())
            {
                _colorTransitionTween.Complete(true);
                _colorTransitionTween = null;
            }

            _alphaTransitionTween = MeshMaterial.DOFade(targetAlpha, _alphaTransitionDuration)
                .OnComplete(() => SetVisibility(!fade));
        }

        private void SetVisibility(bool isVisible)
        {
            _isVisible = isVisible;
            _infoTuple.isVisible = isVisible;
            _infoTuple.segment = this;

            //OnVisibilityChange?.OnNext((isVisible: _isVisible, segment: this));
            OnVisibilityChange?.OnNext(_infoTuple);
        }

        private void ColorChange(bool warning)
        {
            if (MeshMaterial == null) return;

            m_TargetColor = warning ? _warningColor : _baseColor;

            if (_alphaTransitionTween.IsActive())
            {
                _alphaTransitionTween.Complete(true);
                _alphaTransitionTween = null;
            }

            _colorTransitionTween = MeshMaterial.DOColor(m_TargetColor, _colorTransitionDuration)
                .OnComplete(() => 
                { 
                    _isWarning = warning;
                });
        }

        #endregion

        private void OnDestroy()
        {
            if (_colorTransitionTween.IsActive())
            {
                _colorTransitionTween.Complete(true);
                _colorTransitionTween = null;
            }

            if (_alphaTransitionTween.IsActive())
            {
                _alphaTransitionTween.Complete(true);
                _alphaTransitionTween = null;
            }
        }
    }
}
