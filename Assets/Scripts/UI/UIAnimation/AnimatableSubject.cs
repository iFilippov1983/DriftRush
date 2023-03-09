using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace RaceManager.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class AnimatableSubject : MonoBehaviour
    {
        public bool Animate;

        [Title("Animation Settings")]
        [ShowIf("Animate")]
        [SerializeField] private List<AnimationData> _animationsData;

        public Subject<Unit> AnimationInterrupt = new Subject<Unit>();

        public IDisposable Appear(float animationDuration = 0f, RectTransform moveFromRect = null, TweenCallback finishCallback = null)
        {
            if (!Animate)
            {
                Debug.Log($"Animation functions on AnimationSubject [{gameObject.name}] turned off, but you're trying to animate it.");
                return null;
            }

            Sequence appearSequence = DOTween.Sequence();

            foreach (AnimationData data in _animationsData) 
            {
                float duration = animationDuration > 0
                    ? animationDuration
                    : data.animationDuration;

                foreach (var rect in data.rectsToAnimate)
                {
                    Vector3 position = moveFromRect != null 
                        ? moveFromRect.transform.position
                        : data.moveFromToTransform != null
                        ? data.moveFromToTransform.position
                        : rect.transform.position;

                    Tween tween = data.animationType switch
                    {
                        AnimationType.MoveIn => rect.DOMove(position, duration).From(),
                        AnimationType.ScaleFromZero => rect.DOScale(Vector3.zero, duration).From(),
                        _ => null,
                    };

                    if(tween != null)
                        appearSequence.Join(tween);
                }

                foreach (var image in data.imagesToAnimate)
                {
                    Vector3 position = moveFromRect != null
                        ? moveFromRect.transform.position
                        : data.moveFromToTransform != null
                        ? data.moveFromToTransform.position
                        : image.transform.position;

                    Tween tween = data.animationType switch
                    {
                        AnimationType.FadeIn => image.DOFade(0f, duration).From(),
                        AnimationType.MoveIn => image.rectTransform.DOMove(position, duration).From(),
                        AnimationType.ScaleFromZero => image.rectTransform.DOScale(Vector3.zero, duration).From(),
                        _ => null,
                    };

                    if (tween != null)
                        appearSequence.Join(tween);
                }
            }

            if (finishCallback != null)
                appearSequence.AppendCallback(finishCallback);

            AnimationInterrupt.Subscribe(_ =>
            {
                Debug.Log($"Interrupting APPEAR on [{gameObject.name}]");
                appearSequence.Complete();
                appearSequence = null;
            });

            return Disposable.Create(() => appearSequence?.Kill());
        }

        public IDisposable Disappear(float animationDuration = 0f, bool resetOnComplete = true, RectTransform moveToRect = null, TweenCallback finishCallback = null)
        {
            if (!Animate)
            {
                Debug.Log($"Animation functions on AnimationSubject [{gameObject.name}] turned off, but you're trying to animate it.");
                return null;
            }

            Sequence disappearSequence = DOTween.Sequence();

            foreach (AnimationData data in _animationsData)
            {
                float duration = animationDuration > 0
                    ? animationDuration
                    : data.animationDuration;

                foreach (var rect in data.rectsToAnimate)
                {
                    Vector3 initialPos = rect.transform.position;
                    Vector3 initialScale = rect.transform.localScale;
                    Vector3 position = moveToRect != null
                        ? moveToRect.transform.position
                        : data.moveFromToTransform != null
                        ? data.moveFromToTransform.position
                        : rect.transform.position;

                    Tween tween = data.animationType switch
                    {
                        AnimationType.MoveOut => rect.DOMove(position, duration),
                        AnimationType.ScaleToZero => rect.DOScale(Vector3.zero, duration),
                        _ => null,
                    };

                    if (tween != null)
                        disappearSequence.Join(tween);

                    if (resetOnComplete)
                        disappearSequence.AppendCallback(() =>
                        {
                            rect.transform.position = initialPos;
                            rect.transform.localScale = initialScale;
                        });

                    disappearSequence.AppendCallback(() => rect.SetActive(false));
                }

                foreach (var image in data.imagesToAnimate)
                {
                    Color initialColor = image.color;
                    Vector3 initialPos = image.transform.position;
                    Vector3 initialScale = image.transform.localScale;
                    Vector3 position = moveToRect != null
                        ? moveToRect.transform.position
                        : data.moveFromToTransform != null
                        ? data.moveFromToTransform.position
                        : image.transform.position;

                    Tween tween = data.animationType switch
                    {
                        AnimationType.FadeOut => image.DOFade(0f, duration),
                        AnimationType.MoveOut => image.rectTransform.DOMove(position, duration),
                        AnimationType.ScaleToZero => image.rectTransform.DOScale(Vector3.zero, duration),
                        _ => null,
                    }; ;

                    if (tween != null)
                        disappearSequence.Join(tween);

                    if (resetOnComplete)
                        disappearSequence.AppendCallback(() =>
                        {
                            image.transform.position = initialPos;
                            image.transform.localScale = initialScale;
                            image.color = initialColor;
                        });

                    disappearSequence.AppendCallback(() => image.SetActive(false));
                }
            }

            if (finishCallback != null)
                disappearSequence.AppendCallback(finishCallback);

            AnimationInterrupt.Subscribe(_ =>
            {
                Debug.Log($"Interrupting DISAPPEAR on [{gameObject.name}]");
                disappearSequence.Complete();
                disappearSequence = null;
            });

            return Disposable.Create(() => disappearSequence?.Kill());
        }

        public IDisposable Move(float animationDuration = 0f, bool disappearOnComplete = true, Transform moveToTransform = null, TweenCallback finishCallback = null)
        {
            if (!Animate)
            {
                Debug.Log($"Animation functions on AnimationSubject [{gameObject.name}] turned off, but you're trying to animate it.");
                return null;
            }

            Sequence moveSequence = DOTween.Sequence();

            foreach (AnimationData data in _animationsData)
            {
                float duration = animationDuration > 0
                    ? animationDuration
                    : data.animationDuration;

                foreach (var rect in data.rectsToAnimate)
                {
                    Vector3 finishPos = moveToTransform != null
                        ? moveToTransform.position
                        : data.moveFromToTransform != null
                        ? data.moveFromToTransform.position
                        : rect.transform.position;

                    Tween tween = data.animationType switch
                    {
                        AnimationType.MoveFromTo => rect.DOMove(finishPos, duration),
                        _ => null,
                    };

                    if (tween != null)
                        moveSequence.Join(tween);
                }

                foreach (var image in data.imagesToAnimate)
                {
                    Vector3 finishPos = moveToTransform != null
                        ? moveToTransform.position
                        : data.moveFromToTransform != null
                        ? data.moveFromToTransform.position
                        : image.transform.position;

                    Tween tween = data.animationType switch
                    {
                        AnimationType.MoveFromTo => image.rectTransform.DOMove(finishPos, duration),
                        _ => null,
                    }; ;

                    if (tween != null)
                        moveSequence.Join(tween);
                }
            }

            if (disappearOnComplete)
            {
                moveSequence.AppendCallback(() =>
                {
                    Disappear(finishCallback: finishCallback)?.AddTo(this);
                });
            }
            else if (finishCallback != null)
            {
                moveSequence.AppendCallback(finishCallback);
            }

            return Disposable.Create(() => moveSequence?.Kill());
        }

    }
}

