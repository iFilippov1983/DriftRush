using DG.Tweening;
using RaceManager.Progress;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace RaceManager.UI
{
    public class UIAnimator : SerializedMonoBehaviour
    {
        [Header("Currency anim settings")]
        [SerializeField] private int _currencyToSpawnMin = 10;
        [SerializeField] private int _currencyToSpawnMax = 20;
        [SerializeField] private float _currencySpreadFactor = 200f;
        [SerializeField] private float _currencyInOutDuration = 0.25f;
        [SerializeField] private float _currencyMoveDuration = 1f;
        [SerializeField] private float _currencyScrambleDuration = 0.75f;
        [SerializeField] private ScrambleMode _currencyScrambleMode = ScrambleMode.Numerals;
        [DictionaryDrawerSettings(KeyLabel = "Reward Type", ValueLabel = "Rect Size")]
        [SerializeField] private Dictionary<GameUnitType, Vector2> _rectSizes = new Dictionary<GameUnitType, Vector2>();
        [Space]
        [Header("Common rect anim settings")]
        [SerializeField] private float _rectAnimDuration = 0.5f;
        [SerializeField] private float _rectScaleMax = 2f;
        [Space]
        [SerializeField] private float _durationRandomizationFactor = 2f;
        [SerializeField] private float _defaultDuration = 1f;

        private int _counter;

        private SpritesContainerRewards _spritesContainer;
        private GameObject _animCurrencyPrefab;

        private Stack<AnimatableImage> _currenciesStack = new Stack<AnimatableImage>();

        [HideInInspector]
        public Subject<string> ForceCompleteAnimation = new Subject<string>();

        public float DefaultDuration => _defaultDuration;

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
            ForceCompleteAnimation = new Subject<string>();
        }

        public IDisposable SpawnGroupOnAndMoveTo(GameUnitType type, Transform parent, Transform spawnOnTransform, Transform moveToTransform, TweenCallback callback = null)
        {
            switch (type)
            {
                case GameUnitType.Money:
                case GameUnitType.Gems:
                case GameUnitType.CarParts:
                    break;
                default:
                    return Disposable.Empty;
            }

            _counter++;
            float factor = 1f;
            Vector2 size = _rectSizes[type];
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

                
                image.Rect.DOSizeDelta(size, 0f);

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
                spawnSequence.Insert(_currencyMoveDuration * 0.75f, image.transform.DOScale(initialScale / 2, _currencyMoveDuration * 0.25f));
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

                factor += Time.deltaTime * _durationRandomizationFactor;
            }

            ForceCompleteAnimation
                .Where(s => s == type.ToString() && sequences != null)
                .Take(1)
                .Subscribe(i =>
                {
                    foreach (var s in sequences)
                        s.Complete(true);
                    sequences.Clear();
                })
                .AddTo(this);

            return Disposable.Create(() =>
            { 
                foreach(var s in sequences)
                    s?.Kill();
            });
        }

        public IDisposable SpawnGroupOn(GameUnitType type, Transform parent, Transform spawnOnTransform, TweenCallback callback = null)
        {
            switch (type)
            {
                case GameUnitType.Money:
                case GameUnitType.Gems:
                case GameUnitType.CarParts:
                    break;
                default:
                    return Disposable.Empty;
            }

            _counter++;
            Vector2 size = _rectSizes[type];
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


                image.Rect.DOSizeDelta(size, 0f);

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
                spawnSequence.AppendCallback(i == 0 ? callback : null);

                spawnSequence.AppendCallback(() =>
                {
                    _currenciesStack.Push(image);
                });

                sequences.Add(spawnSequence);
                spawnSequence.Play();
            }

            ForceCompleteAnimation
                .Where(s => s == type.ToString() && sequences != null)
                .Take(1)
                .Subscribe(i =>
                {
                    foreach (var s in sequences)
                        s.Complete(true);
                    sequences.Clear();
                })
                .AddTo(this);

            return Disposable.Create(() =>
            {
                foreach (var s in sequences)
                    s?.Kill();
            });
        }

        public IDisposable ScrambleNumeralsText(TMP_Text textToScramble, string endTextValue)
        {
            Tween tween = textToScramble.DOText(endTextValue, _currencyScrambleDuration, true, _currencyScrambleMode);

            ForceCompleteAnimation
                .Where(s => s == textToScramble?.name && tween != null)
                .Take(1)
                .Subscribe(i => 
                { 
                    tween.Complete(true);
                    tween = null;
                })
                .AddTo(this);

            return Disposable.Create(() => tween?.Kill());
        }

        public IDisposable RectAnimate(RectTransform rect, TweenCallback finishCallBack = null, params AnimationType[] animationsSequence)
        { 
            Sequence animSequence = DOTween.Sequence();

            Vector3 targetScale = rect.transform.localScale * _rectScaleMax;
            Vector3 initialScale = rect.transform.localScale;

            foreach (var animation in animationsSequence)
            {
                Tween tween = animation switch
                {
                    AnimationType.ScaleFromZero => rect.transform.DOScale(0f, _rectAnimDuration).From(),
                    AnimationType.ScaleToZero => rect.transform.DOScale(0f, _rectAnimDuration),
                    AnimationType.PunchScale => rect.transform.DOPunchScale(targetScale, _rectAnimDuration),
                    AnimationType.ShakeScale => rect.transform.DOShakeScale(_rectAnimDuration),
                    _ => null,
                };

                if(tween != null)
                    animSequence.Append(tween);
            }

            if(finishCallBack != null)
                animSequence.AppendCallback(finishCallBack);

            animSequence.AppendCallback(() => rect.transform.localScale = initialScale);

            ForceCompleteAnimation
                .Where(s => s == rect.name && animSequence != null)
                .Take(1)
                .Subscribe(x =>
                {
                    animSequence.Complete(true);
                    animSequence = null;
                })
                .AddTo(this);

            return Disposable.Create(() => animSequence?.Kill());
        }

        public IDisposable RectMoveTo(RectTransform rect, Transform moveToPos, bool resetOnFinish = true, bool disableOnFinish = true)
        {
            Vector3 initialPos = rect.transform.position;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(rect.DOMove(moveToPos.position, _rectAnimDuration));

            if (resetOnFinish)
                sequence.AppendCallback(() => rect.transform.position = initialPos);

            if (disableOnFinish)
                sequence.AppendCallback(() => rect.transform.SetActive(false));

            ForceCompleteAnimation
                .Where(s => s == rect.name && sequence != null)
                .Take(1)
                .Subscribe(s =>
                {
                    sequence.Complete(true);
                    sequence = null;
                })
                .AddTo(this);

            return Disposable.Create(() => sequence?.Kill());
        }

        public IDisposable RectMoveFrom(RectTransform rect, Transform moveFromPos)
        {
            Tween tween = rect.DOMove(moveFromPos.position, _rectAnimDuration).From();

            ForceCompleteAnimation
                .Where(s => s == rect.name && tween != null)
                .Take(1)
                .Subscribe(s =>
                {
                    tween.Complete(true);
                    tween = null;
                });

            return Disposable.Create(() => tween?.Kill());
        }

        public IDisposable AppearSubject(IAnimatableSubject subject, Transform moveFromTransform = null, TweenCallback finishCallback = null)
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

                    Vector3 yEqual = new Vector3(position.x, rect.transform.position.y, position.z);
                    Vector3 xEqual = new Vector3(rect.transform.position.x, position.y, position.z);

                    Tween tween = data.animationType switch
                    {
                        AnimationType.MoveIn => rect.DOMove(position, duration).From(),
                        AnimationType.MoveInX => rect.DOMove(yEqual, duration).From(),
                        AnimationType.MoveInY => rect.DOMove(xEqual, duration).From(),
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

                    Vector3 yEqual = new Vector3(position.x, image.rectTransform.transform.position.y, position.z);
                    Vector3 xEqual = new Vector3(image.rectTransform.transform.position.x, position.y, position.z);

                    Tween tween = data.animationType switch
                    {
                        AnimationType.FadeIn => image.DOFade(0f, duration).From(),
                        AnimationType.MoveIn => image.rectTransform.DOMove(position, duration).From(),
                        AnimationType.MoveInX => image.rectTransform.DOMove(yEqual, duration).From(),
                        AnimationType.MoveInY => image.rectTransform.DOMove(xEqual, duration).From(),
                        AnimationType.ScaleFromZero => image.rectTransform.DOScale(Vector3.zero, duration).From(),
                        _ => null,
                    };

                    if (tween != null)
                        appearSequence.Join(tween);
                }
            }

            if (finishCallback != null)
                appearSequence.AppendCallback(finishCallback);

            ForceCompleteAnimation
                .Where(s => s == subject?.Name && appearSequence != null)
                .Take(1)
                .Subscribe(_ =>
                { 
                    appearSequence.Complete(true);
                    appearSequence = null;
                })
                .AddTo(this);

            return Disposable.Create(() => appearSequence?.Kill());
        }

        public IDisposable DisappearSubject(IAnimatableSubject subject, Transform moveToTransform = null, bool resetOnComplete = true, bool disableOnComplete = false, TweenCallback finishCallback = null)
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

                    Vector3 yEqual = new Vector3(position.x, rect.transform.position.y, position.z);
                    Vector3 xEqual = new Vector3(rect.transform.position.x, position.y, position.z);

                    Tween tween = data.animationType switch
                    {
                        AnimationType.MoveOut => rect.DOMove(position, duration),
                        AnimationType.MoveOutX => rect.DOMove(yEqual, duration),
                        AnimationType.MoveOutY => rect.DOMove(xEqual, duration),
                        AnimationType.ScaleToZero => rect.DOScale(Vector3.zero, duration),
                        _ => null,
                    };

                    if (tween != null)
                    {
                        disappearSequence.Join(tween);
                        if (resetOnComplete)
                            disappearSequence.AppendCallback(() =>
                            {
                                rect.transform.position = initialPos;
                                rect.transform.localScale = initialScale;
                            });

                        if(disableOnComplete)
                            disappearSequence.AppendCallback(() => rect.SetActive(false));
                    }
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

                    Vector3 yEqual = new Vector3(position.x, image.rectTransform.transform.position.y, position.z);
                    Vector3 xEqual = new Vector3(image.rectTransform.transform.position.x, position.y, position.z);

                    Tween tween = data.animationType switch
                    {
                        AnimationType.FadeOut => image.DOFade(0f, duration),
                        AnimationType.MoveOut => image.rectTransform.DOMove(position, duration),
                        AnimationType.MoveOutX => image.rectTransform.DOMove(yEqual, duration),
                        AnimationType.MoveOutY => image.rectTransform.DOMove(xEqual, duration),
                        AnimationType.ScaleToZero => image.rectTransform.DOScale(Vector3.zero, duration),
                        _ => null,
                    }; ;

                    if (tween != null)
                    {
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
            }

            if (finishCallback != null)
                disappearSequence.AppendCallback(finishCallback);

            ForceCompleteAnimation
                .Where(s => s == subject?.Name && disappearSequence != null)
                .Take(1)
                .Subscribe(_ =>
                { 
                    disappearSequence.Complete(true);
                    disappearSequence = null;
                })
                .AddTo(this);

            return Disposable.Create(() => disappearSequence?.Kill());
        }
    }
}

