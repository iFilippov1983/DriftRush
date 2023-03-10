using DG.Tweening;
using RaceManager.Progress;
using RaceManager.Tools;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace RaceManager.UI
{
    public class UIAnimator : MonoBehaviour
    {
        [Header("Currency anim settings")]
        [SerializeField] private int _currencyToSpawnMin = 10;
        [SerializeField] private int _currencyToSpawnMax = 20;
        [SerializeField] private float _currencySpreadFactor = 200f;
        [SerializeField] private float _currencyInOutDuration = 0.25f;
        [SerializeField] private float _currencyMoveDuration = 1f;
        [SerializeField] private float _currencyScrambleDuration = 0.75f;
        [SerializeField] private ScrambleMode _currencyScrambleMode = ScrambleMode.Numerals;
        [Space]
        [Header("UI marks anim settings")]
        [SerializeField] private float _markInOutDuration = 1f;
        [SerializeField] private float _markScaleMax = 2f;
        [Space]
        [Header("Cards representation anim settings")]
        [SerializeField] private float _cardInOutSpeed = 0.25f;
        [SerializeField] private float _cardMessageDuration = 1f;
        [SerializeField] private float _cardMoveDuration = 1f;
        [Space]
        [SerializeField] private float _durationRandFactor = 2f;

        private int _counter;

        private SpritesContainerRewards _spritesContainer;
        private GameObject _animCurrencyPrefab;

        private Stack<AnimatableImage> _currenciesStack = new Stack<AnimatableImage>();

        public Subject<string> InterruptAnimation = new Subject<string>();

        private GameObject AnimCurrencyPrefab
        {
            get
            {
                if (_animCurrencyPrefab == null)
                    _animCurrencyPrefab = ResourcesLoader.LoadPrefab(ResourcePath.AnimatableCurrencyPrefab);
                return _animCurrencyPrefab;
            }
        }

        [Inject]
        private void Construct(SpritesContainerRewards spritesContainer)
        { 
            _spritesContainer = spritesContainer;
        }

        public IDisposable SpawnCurrencyOnAndMoveTo(RewardType type, Transform parent, Transform spawnOnTransform, Transform moveToTransform, TweenCallback callback = null)
        {
            _counter++;
            float factor = 1f;
            Sprite sprite = _spritesContainer.GetColoredRewardSprite(type);
            List<Sequence> sequences = new List<Sequence>();

            int amount = Random.Range(_currencyToSpawnMin, _currencyToSpawnMax);
            for (int i = 0; i < amount; i++)
            {
                AnimatableImage image;

                if (_currenciesStack.Count > 0)
                {
                    image = _currenciesStack.Pop();
                }
                else
                {
                    GameObject imageGo = Instantiate(AnimCurrencyPrefab, gameObject.transform);
                    imageGo.name = $"image [{_counter}]";

                    image = imageGo.GetComponent<AnimatableImage>();
                }

                image.SetActive(true);
                image.Image.sprite = sprite;
                image.transform.SetParent(parent, false);
                image.transform.position = spawnOnTransform.position;
                
                Vector3 initialScale = image.transform.localScale;

                Vector3 shiftPos = Random.insideUnitCircle * _currencySpreadFactor;
                shiftPos += spawnOnTransform.position;

                Sequence spawnSequence = DOTween.Sequence();

                spawnSequence.Append(image.transform.DOMove(shiftPos, _currencyInOutDuration));
                spawnSequence.Join(image.transform.DOScale(0f, _currencyInOutDuration).From());
                spawnSequence.AppendInterval(_currencyInOutDuration / 2);
                spawnSequence.Append(image.transform.DOMove(moveToTransform.position, _currencyMoveDuration * factor));
                spawnSequence.Insert(_currencyMoveDuration / 2, image.transform.DOScale(initialScale / 2, _currencyMoveDuration / 2));
                spawnSequence.AppendCallback(i == 0 ? callback : null);
                spawnSequence.Append(image.transform.DOScale(0f, _currencyInOutDuration));
                spawnSequence.AppendCallback(() => 
                { 
                    image.transform.localScale = initialScale;
                    image.SetActive(false);
                    _currenciesStack.Push(image);
                });

                sequences.Add(spawnSequence);
                spawnSequence.Play();

                factor += Time.deltaTime * _durationRandFactor;
            }

            return Disposable.Create(() =>
            { 
                foreach(var s in sequences)
                    s?.Kill();
            });
        }

        public IDisposable ScrambleNumeralsText(TMP_Text textToScramble, string endTextValue)
        {
            Tween tween = textToScramble.DOText(endTextValue, _currencyScrambleDuration, true, _currencyScrambleMode);

            return Disposable.Create(() => tween?.Kill());
        }

        public IDisposable Appear(IAnimatableSubject subject, Transform moveFromTransform = null, TweenCallback finishCallback = null)
        {
            Sequence appearSequence = DOTween.Sequence();

            foreach (AnimationData data in subject.AnimationDataList)
            {
                float duration = data.animationDuration;

                foreach (var rect in data.rectsToAnimate)
                {
                    Vector3 position = moveFromTransform != null
                        ? moveFromTransform.position
                        : data.moveFromToTransform != null
                        ? data.moveFromToTransform.position
                        : rect.transform.position;

                    Tween tween = data.animationType switch
                    {
                        AnimationType.MoveIn => rect.DOMove(position, duration).From(),
                        AnimationType.ScaleFromZero => rect.DOScale(Vector3.zero, duration).From(),
                        _ => null,
                    };

                    if (tween != null)
                        appearSequence.Join(tween);
                }

                foreach (var image in data.imagesToAnimate)
                {
                    Vector3 position = moveFromTransform != null
                        ? moveFromTransform.position
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

            InterruptAnimation
                .Where(s => s == subject.Name)
                .Take(1)
                .Subscribe(_ =>
                { 
                    appearSequence.Complete(true);
                    appearSequence = null;
                });

            return Disposable.Create(() => appearSequence?.Kill());
        }

        public IDisposable Disappear(IAnimatableSubject subject, Transform moveToTransform = null, bool resetOnComplete = true, TweenCallback finishCallback = null)
        {
            Sequence disappearSequence = DOTween.Sequence();

            foreach (AnimationData data in subject.AnimationDataList)
            {
                float duration = data.animationDuration;

                foreach (var rect in data.rectsToAnimate)
                {
                    Vector3 initialPos = rect.transform.position;
                    Vector3 initialScale = rect.transform.localScale;
                    Vector3 position = moveToTransform != null
                        ? moveToTransform.transform.position
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
                    Vector3 position = moveToTransform != null
                        ? moveToTransform.transform.position
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

            InterruptAnimation
                .Where(s => s == subject.Name)
                .Take(1)
                .Subscribe(_ =>
                { 
                    disappearSequence.Complete(true);
                    disappearSequence = null;
                });

            return Disposable.Create(() => disappearSequence?.Kill());
        }
    }
}

