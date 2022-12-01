using System;
using UnityEngine;
using DG.Tweening;

namespace RaceManager.UI
{
    public class ImageAnimationHandler
    {
        private const float Duration = 1f;
        private readonly Vector3 ScaleVectorBig = new Vector3(1.3f, 1.3f, 1.3f);
        private readonly Vector3 ScaleVectorSmall = new Vector3(0.85f, 0.85f, 0.85f);

        private GameObject _imageToAnimate;

        public event Action OnAnimationInitialize;
        public event Action OnAnimationFinish;

        private Vector3 _initialPos;

        public ImageAnimationHandler(GameObject imageToAnimate)
        {
            _imageToAnimate = imageToAnimate;
            _initialPos = _imageToAnimate.transform.position;
        }

        public void InitializeAnimationWithTarget(GameObject target, Vector3 posOffset)
        {
            OnAnimationInitialize?.Invoke();
            _imageToAnimate.transform.DOMove(target.transform.position + posOffset, Duration)
                .OnComplete(FinishAnimation);
            _imageToAnimate.transform.DOScale(ScaleVectorBig, Duration / 2)
                .OnComplete(() => _imageToAnimate.transform.DOScale(ScaleVectorSmall, Duration / 2));
        }

        private void FinishAnimation()
        {
            _imageToAnimate.transform.position = _initialPos;
            _imageToAnimate.transform.DOScale(Vector3.one, Duration / 2);
            OnAnimationFinish?.Invoke();
        }
    }
}

