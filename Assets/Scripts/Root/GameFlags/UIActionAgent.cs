using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using RaceManager.UI;
using TMPro;
using Zenject;
using UniRx;
using Sirenix.OdinInspector;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Threading.Tasks;
using static Logger;

namespace RaceManager.Root
{
    public class UIActionAgent : MonoBehaviour, IInitializable
    {
        [Serializable]
        public class AgentAction
        {
            public GameFlagType key;
            [Space(20)]
            public List<AgentActionType> actions = new List<AgentActionType>();
            [Space]
            [ShowIf("UseAnimations")]
            public List<AnimationType> animationTypes = new List<AnimationType>();
            [Space]
            [ShowIf("UseTweenAnimations")]
            public List<DOTweenAnimation> tweens = new List<DOTweenAnimation>();
            [ShowIf("NeedToSetAnimations")]
            public float animationDuration = 1f;
            [ShowIf("NeedToSetAnimations")]
            public float actionStartDelay = 0f;

            [Header("Fade In/Out Settings")]
            [ShowIf("UseFadeInOut")]
            [Range(0f, 1f)]
            public float minAlpha;
            [ShowIf("UseFadeInOut")]
            [Range(0f, 1f)]
            public float maxAlpha = 1f;

            [Header("Scale Up/Down Setings")]
            [ShowIf("UseScaleUpDown")]
            [Range(0f, 10f)]
            public float minScale = 1f;
            [ShowIf("UseScaleUpDown")]
            [Range(0f, 10f)]
            public float maxScale = 1.05f;

            [Header("Move From/To Settings")]
            [ShowIf("UseMoveFromTo")]
            public Transform fromPosition;
            [ShowIf("UseMoveFromTo")]
            public Transform toPosition;
            [ShowIf("UseMoveFromTo")]
            [Range(0.01f, 10f)]
            public float moveDurationFactor = 1f;
            [ShowIf("UseMoveFromTo")]
            public bool loopMove;

            #region ShowIf Editor Properties

            public bool UseAnimations =>
                actions.Contains(AgentActionType.StartAnimation) || NeedToStopAnimation;
            public bool NeedToSetAnimations =>
                actions.Contains(AgentActionType.StartAnimation) && !NeedToStopAnimation;
            public bool NeedToStopAnimation =>
                actions.Contains(AgentActionType.StopAnimation);
            public bool UseFadeInOut =>
                animationTypes.Contains(AnimationType.FadeInOutLoop) && NeedToSetAnimations;
            public bool UseScaleUpDown =>
                animationTypes.Contains(AnimationType.ScaleUpDownLoop) && NeedToSetAnimations;
            public bool UseMoveFromTo =>
                animationTypes.Contains(AnimationType.MoveFromTo) && NeedToSetAnimations;
            public bool UseTweenAnimations => 
                actions.Contains(AgentActionType.PlayTweenAnimation) || actions.Contains(AgentActionType.StopTweenAnimation);

            #endregion
        }

        [Header("Flags Actions")]
        [SerializeField] private List<AgentAction> _agentActions = new List<AgentAction>();
        
        private GameFlagsHandler _flagsHandler;
        private Button _button;
        private Image _image;
        private TMP_Text _text;

        private Vector3 _originalScale = default;
        private Vector3 _originalPosition = default;
        private float _originalAlpha = default;

        public Subject<AnimationType> StopAnimationSubject = new Subject<AnimationType>(); 

        public AgentType Type { get; private set; }

        [Inject]
        protected virtual void Construct(GameFlagsHandler flagsHandler, Resolver resolver) 
        {
            resolver.Add(this);
            _flagsHandler = flagsHandler;
            _originalScale = transform.localScale;
            _originalPosition = transform.position;

            FindProperComponent();
        }

        public void Initialize()
        {
            foreach (var aAction in _agentActions)
            {
                foreach (var action in aAction.actions)
                {
                    switch (action)
                    {
                        case AgentActionType.Click:
                            ClickOnFlag(aAction);
                            break;
                        case AgentActionType.InteractableTrue:
                            InteractableOnFlag(true, aAction);
                            break;
                        case AgentActionType.InteractableFalse:
                            InteractableOnFlag(false, aAction);
                            break;
                        case AgentActionType.StartAnimation:
                            ToggleAnimationOnFlag(true, aAction);
                            break;
                        case AgentActionType.StopAnimation:
                            ToggleAnimationOnFlag(false, aAction);
                            break;
                        case AgentActionType.PlayTweenAnimation:
                            ToggleTweenOnFlag(true, aAction);
                            break;
                        case AgentActionType.StopTweenAnimation:
                            ToggleTweenOnFlag(false, aAction);
                            break;
                    }
                }
            }
        }

        private bool TimeForAction(GameFlagType key) => _flagsHandler.HasFlag(key);

        private bool IsLastKey(GameFlagType key) => _flagsHandler.IsLast(key);

        private void ClickOnFlag(AgentAction aAction)
        {
            if (Type != AgentType.Button)
            {
                Debug.LogError($"UI Action Agent [{gameObject.name}] has no any Button component, but you whant to click it!");
                return;
            }

            if (TimeForAction(aAction.key))
            { 
                if(IsLastKey(aAction.key))
                    ClickAgent(aAction.actionStartDelay);

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, () => ClickAgent(aAction.actionStartDelay))
                .AddTo(this);

            Debug.Log($"ClickOnFlag subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]");
        }

        private void InteractableOnFlag(bool interactable, AgentAction aAction)
        {
            if (Type != AgentType.Button)
            {
                Debug.LogError($"UI Action Agent [{gameObject.name}] has no any Button component, but you whant to set its Interactable!");
                return;
            }

            if (TimeForAction(aAction.key))
            {
                if (IsLastKey(aAction.key))
                    MakeButtonInteractable(interactable);

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, () => MakeButtonInteractable(interactable))
                .AddTo(this);

            Debug.Log($"InteractableOnFlag ({interactable}) subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]");
        }

        private void ToggleAnimationOnFlag(bool start, AgentAction aAction)
        {
            bool incorrectAnimationTypes =
                aAction.animationTypes.Count == 0 || aAction.animationTypes.Contains(AnimationType.None);

            if (start && incorrectAnimationTypes)
            {
                Debug.LogError($"You're trying to animate UI Action Agent [{gameObject.name}] but you haven't assigned correct animation type!");
                return;
            }

            if (TimeForAction(aAction.key))
            {
                if (IsLastKey(aAction.key))
                    ToggleAnimationsStatus(start, aAction);

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, () => ToggleAnimationsStatus(start, aAction))
                .AddTo(this);

            Debug.Log($"ToggleAnimationOnFlag ({start}) subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]");
        }

        private void ToggleTweenOnFlag(bool play, AgentAction aAction)
        { 
            bool noTweens = aAction.tweens.Count == 0;

            if (play && noTweens) 
            {
                Debug.LogError($"DOTweenAnimation List on UI Action Agent [{gameObject.name}] is empty!");
                return;
            }

            if (TimeForAction(aAction.key))
            { 
                if(IsLastKey(aAction.key))
                    ToggleTweensStatus(play, aAction);

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, () => ToggleTweensStatus(play, aAction))
                .AddTo(this);

            Debug.Log($"ToggleTweenOnFlag ({play}) subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]");
        }

        private async void ClickAgent(float actionDelaySeconds = 0)
        {
            int delay = Convert.ToInt32(actionDelaySeconds * 1000);
            await Task.Delay(delay);

            _button.onClick?.Invoke();
        }

        private async void MakeButtonInteractable(bool interactable, float actionDelaySeconds = 0)
        {
            int delay = Convert.ToInt32(actionDelaySeconds * 1000);
            await Task.Delay(delay);

            _button.interactable = interactable;
        }

        private async void ToggleAnimationsStatus(bool start, AgentAction aAction, float actionDelaySeconds = 0)
        {
            int delay = Convert.ToInt32(actionDelaySeconds * 1000);
            await Task.Delay(delay);

            foreach (var animationType in aAction.animationTypes)
            {
                if (animationType != AnimationType.None)
                    StartAnimation(start, animationType, aAction);
            }
        }

        private async void ToggleTweensStatus(bool play, AgentAction aAction, float actionDelaySeconds = 0)
        {
            int delay = Convert.ToInt32(actionDelaySeconds * 1000);
            await Task.Delay(delay);

            foreach (var tween in aAction.tweens)
            {
                if(play)
                    tween.DOPlay();
                else
                    tween.DOKill();
            }
        }

        private async void StartAnimation(bool start, AnimationType type, AgentAction aAction)
        {
            IEnumerator animCoroutine = type switch
            {
                AnimationType.None => null,
                AnimationType.FadeInOutLoop => FadeInOut(aAction),
                AnimationType.ScaleUpDownLoop => ScaleUpDown(aAction),
                AnimationType.MoveFromTo => MoveFromTo(aAction),
                _ => null,
            };

            if (start)
            {
                while (!gameObject.activeInHierarchy)
                {
                    await Task.Yield();
                }

#pragma warning disable CS4014 
                StartCoroutine(animCoroutine);
#pragma warning restore CS4014 
            }
            else
            {
                StopAnimationSubject.OnNext(type);

                if (animCoroutine != null)
                {
                    StopCoroutine(animCoroutine);
                }
                    
                //switch (type)
                //{
                //    case AnimationType.None:
                //        break;
                //    case AnimationType.FadeInOutLoop:
                //        //ResetAlpha();
                //        break;
                //    case AnimationType.ScaleUpDownLoop:
                //        //ResetScale();
                //        break;
                //    case AnimationType.MoveFromTo:
                //        //ResetPosition();
                //        break;
                //}
            }
        }

        private IEnumerator FadeInOut(AgentAction aAction)
        {
            yield return new WaitForSeconds(aAction.actionStartDelay);

            bool fade = true;
            float targetAlpha;
            TweenerCore<Color, Color, ColorOptions> tween = null;

            bool animate = true;

            StopAnimationSubject
                .Where(t => t == AnimationType.FadeInOutLoop)
                .Subscribe(t => 
                {
                    animate = false;
                    tween?.Kill();
                    tween = null;
                    targetAlpha = _originalAlpha;
                    tween = DoFade(targetAlpha, 0);
                })
                .AddTo(this);

            while (animate)
            {
                targetAlpha = fade ? aAction.minAlpha : aAction.maxAlpha;

                tween = DoFade(targetAlpha, aAction.animationDuration);
                tween.OnComplete(() => 
                {
                    fade = !fade;
                    tween = null;
                });
                

                tween.onUpdate += () => ToggleActivity(tween);

                while (tween.IsActive())
                {
                    yield return null;
                }

                tween.onUpdate -= () => ToggleActivity(tween);
            }
        }

        private TweenerCore<Color, Color, ColorOptions> DoFade(float targetAlpha, float animationDuration)
        {
            return Type switch
            {
                AgentType.None => null,
                AgentType.Button => _button.image.DOFade(targetAlpha, animationDuration),
                AgentType.Image => _image.DOFade(targetAlpha, animationDuration),
                AgentType.Text => _text.DOFade(targetAlpha, animationDuration),
                _ => null
            };
        }

        private IEnumerator ScaleUpDown(AgentAction aAction)
        {
            yield return new WaitForSeconds(aAction.actionStartDelay);

            bool scaleDown = false;
            float targetScale;
            TweenerCore<Vector3, Vector3, VectorOptions> tween = null;

            bool animate = true;

            StopAnimationSubject
                .Where(t => t == AnimationType.ScaleUpDownLoop)
                .Subscribe(t => 
                { 
                    animate = false;
                    tween?.Kill();
                    tween = null;
                    targetScale = _originalScale.x;
                    tween = transform.DOScale(targetScale, aAction.animationDuration);

                    //ResetScale();
                })
                .AddTo(this);

            while (animate)
            {
                targetScale = scaleDown ? aAction.minScale : aAction.maxScale;

                tween = transform.DOScale(targetScale, aAction.animationDuration)
                    .OnComplete(() => scaleDown = !scaleDown);

                while (tween.IsActive())
                {
                    yield return null;
                }
            }
        }

        private IEnumerator MoveFromTo(AgentAction aAction)
        {
            yield return new WaitForSeconds(aAction.actionStartDelay);

            Vector3 originPos = transform.position;
            Vector3 fromPosition = aAction.fromPosition.position;
            Vector3 toPosition = aAction.toPosition.position;

            TweenerCore<Vector3, Vector3, VectorOptions> tween = null;

            bool animate = true;

            StopAnimationSubject
                .Where(t => t == AnimationType.MoveFromTo)
                .Subscribe(t =>
                {
                    animate = false;
                    tween?.Kill();
                    tween = null;
                    //tween = transform.DOMove(originPos, aAction.animationDuration * aAction.moveDurationFactor);
                    transform.position = originPos;
                    //ResetPosition();
                })
                .AddTo(this);

            while (animate)
            {
                transform.position = fromPosition;

                tween = transform.DOMove(toPosition, aAction.animationDuration * aAction.moveDurationFactor);

                while (tween.IsActive())
                {
                    yield return null;
                }

                
                if (!aAction.loopMove) break;
            }
        }

        private void ToggleActivity(TweenerCore<Color, Color, ColorOptions> tween)
        {
            if (gameObject.activeSelf && !tween.IsPlaying())
                tween.Restart();
            else
                tween.Pause();
        }

        private Color GetColor(AgentType type)
        {
            Color color = type switch
            {
                AgentType.None => throw new InvalidCastException(),
                AgentType.Button => _button.image.color,
                AgentType.Image => _image.color,
                AgentType.Text => _text.color,
                _ => throw new InvalidCastException()
            };

            return color;
        }

        private void SetColor(AgentType type, Color color)
        {
            _ = type switch
            {
                AgentType.None => throw new InvalidCastException(),
                AgentType.Button => _button.image.color = color,
                AgentType.Image => _image.color = color,
                AgentType.Text => _text.color = color,
                _ => throw new InvalidCastException()
            };
        }

        private void ResetAlpha()
        {
            Color color = GetColor(Type);
            color.a = _originalAlpha;
            SetColor(Type, color);

            Debug.Log($"Alpha is set to {color.a} for [{gameObject.name}]");
        }

        private void ResetScale() => transform.localScale = _originalScale;

        private void ResetPosition() => transform.position = _originalPosition;

        private void FindProperComponent()
        {
            bool found;

            found = TryGetComponent(out _button);
            if (found)
            {
                _originalAlpha = _button.image.color.a;

                Type = AgentType.Button;

                return;
            }

            found = TryGetComponent(out _image);
            if (found)
            {
                _originalAlpha = _image.color.a;

                Type = AgentType.Image;

                return;
            }

            found = TryGetComponent(out _text);
            if (found)
            {
                _originalAlpha = _text.alpha;

                Type = AgentType.Text;

                return;
            }

            Type = AgentType.None;
            Debug.LogWarning($"UI Flag Agent [{gameObject.name}] has no porper components to work with!");
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
