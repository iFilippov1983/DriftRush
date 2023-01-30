using System;
using UnityEngine;
using DG.Tweening;

namespace RaceManager.UI
{
    public class LootboxImageAnimationHandler
    {
        private readonly float Duration = 1f;
        private readonly Vector3 ScaleVectorBig = new Vector3(1.5f, 1.5f, 1.5f);
        private readonly Vector3 ScaleVectorSmall = new Vector3(0.85f, 0.85f, 0.85f);

        private GameObject _imageToAnimate;
        private Vector3 _initialPos;

        public event Action OnAnimationInitialize;
        public event Action OnAnimationFinish;

        public LootboxImageAnimationHandler(GameObject imageToAnimate)
        {
            _imageToAnimate = imageToAnimate;
            _initialPos = _imageToAnimate.transform.position;
        }

        public LootboxImageAnimationHandler(float duration, Vector3 scaleVectorBig, Vector3 scaleVectorSmall, GameObject imageToAnimate)
        {
            Duration = duration;
            ScaleVectorBig = scaleVectorBig;
            ScaleVectorSmall = scaleVectorSmall;

            _imageToAnimate = imageToAnimate;
            _initialPos = _imageToAnimate.transform.position;
        }

        public void InitializeAnimationWithTarget(GameObject target, Vector3 targetPosOffset)
        {
            OnAnimationInitialize?.Invoke();
            _imageToAnimate.transform.DOScale(ScaleVectorBig, Duration / 2).OnComplete(() =>
            {
                _imageToAnimate.transform.DOMove(target.transform.position + targetPosOffset, Duration);
                _imageToAnimate.transform.DOScale(ScaleVectorSmall, Duration)
                .OnComplete(FinishAnimation);
            });
        }

        private void FinishAnimation()
        {
            OnAnimationFinish?.Invoke();
            _imageToAnimate.transform.localScale = Vector3.zero;
            _imageToAnimate.transform.position = _initialPos;
            _imageToAnimate.transform.DOScale(Vector3.one, Duration / 2);
        }
    }
}

