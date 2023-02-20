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

            #endregion

            #region Debug fields

            //[Header("Only for debug")]
            //[ShowInInspector, ReadOnly]
            [HideInInspector]
            public IEnumerator CurrentFadeJob;
            //[ShowInInspector, ReadOnly]
            [HideInInspector]
            public IEnumerator CurrentScaleJob;
            //[ShowInInspector, ReadOnly]
            [HideInInspector]
            public IEnumerator CurrentMoveJob;

            #endregion
        }

        [Header("Flags Actions")]
        [SerializeField] private List<AgentAction> _agentActions = new List<AgentAction>();

        public bool useToRemind;

        [ShowIf("useToRemind")]
        [Header("Reminder Actions")]
        [SerializeField]
        private AgentAction _reminderAction = new AgentAction();
        
        private GameFlagsHandler _flagsHandler;
        private Button _button;
        private Image _image;
        private TMP_Text _text;

        private Vector3 _originalScale = default;
        private Vector3 _originalPosition = default;
        private float _originalAlpha = default;

        private bool _isDead = false;

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
                    }
                }
            }
        }

        public async Task Click()
        {
            await ClickAgent(_reminderAction.actionStartDelay);
        }

        public async Task ButtonInteractable(bool interactable)
        {
            await MakeButtonInteractable(interactable, _reminderAction.actionStartDelay);
        }

        public async Task StartAnimation()
        {
            await ToggleAnimationsStatus(true, _reminderAction, _reminderAction.actionStartDelay);
        }

        public async Task StopAnimation()
        {
            await ToggleAnimationsStatus(false, _reminderAction, _reminderAction.actionStartDelay);
        }

        private bool TimeForAction(GameFlagType key) => _flagsHandler.HasFlag(key);

        private bool IsLastKey(GameFlagType key) => _flagsHandler.IsLast(key);

        private async void ClickOnFlag(AgentAction aAction)
        {
            if (Type != AgentType.Button)
            {
                $"UI Flag Agent [{gameObject.name}] has no any Button component, but you whant to click it!".Log(ColorRed);
                return;
            }

            if (TimeForAction(aAction.key))
            { 
                if(IsLastKey(aAction.key))
                    await ClickAgent(aAction.actionStartDelay);

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, async () => await ClickAgent(aAction.actionStartDelay))
                .AddTo(this);

            $"ClickOnFlag subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]".Log(ColorBlue);
        }

        private async void InteractableOnFlag(bool interactable, AgentAction aAction)
        {
            if (Type != AgentType.Button)
            {
                $"UI Flag Agent [{gameObject.name}] has no any Button component, but you whant to set its Interactable to True!".Log(ColorRed);
                return;
            }

            if (TimeForAction(aAction.key))
            {
                if (IsLastKey(aAction.key))
                    await MakeButtonInteractable(interactable);

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, async () => await MakeButtonInteractable(interactable))
                .AddTo(this);

            $"InteractableOnFlag ({interactable}) subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]".Log(ColorBlue);
        }

        private async void ToggleAnimationOnFlag(bool start, AgentAction aAction)
        {
            bool incorrectAnimationTypes =
                aAction.animationTypes.Count == 0 || aAction.animationTypes.Contains(AnimationType.None);

            if (start && incorrectAnimationTypes)
            {
                $"You're trying to animate UI Flag Agent [{gameObject.name}] but you haven't assigned correct animation type!".Log(ColorRed);
                return;
            }

            if (TimeForAction(aAction.key))
            {
                if (IsLastKey(aAction.key))
                    await ToggleAnimationsStatus(start, aAction);

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, async () => await ToggleAnimationsStatus(start, aAction))
                .AddTo(this);

            $"ToggleAnimationOnFlag ({start}) subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]".Log(ColorBlue);
        }

        private async Task ClickAgent(float actionDelaySeconds = 0)
        {
            int delay = Convert.ToInt32(actionDelaySeconds * 1000);
            await Task.Delay(delay);

            _button.onClick?.Invoke();
        }

        private async Task MakeButtonInteractable(bool interactable, float actionDelaySeconds = 0)
        {
            int delay = Convert.ToInt32(actionDelaySeconds * 1000);
            await Task.Delay(delay);

            _button.interactable = interactable;
        }

        private async Task ToggleAnimationsStatus(bool start, AgentAction aAction, float actionDelaySeconds = 0)
        {
            int delay = Convert.ToInt32(actionDelaySeconds * 1000);
            await Task.Delay(delay);

            foreach (var animationType in aAction.animationTypes)
            {
                if (animationType == AnimationType.None)
                {
                    $"AnimationType [None] detected on UI Flag Agent [{gameObject.name}] => Key [{aAction.key}]".Log(ColorYellow);
                    continue;
                }

                StartAnimation(start, animationType, aAction);
            }
        }

        private async void StartAnimation(bool start, AnimationType type, AgentAction aAction)
        {
            IEnumerator currentAnimJob = type switch
            {
                AnimationType.None => null,
                AnimationType.FadeInOutLoop => aAction.CurrentFadeJob,
                AnimationType.ScaleUpDownLoop => aAction.CurrentScaleJob,
                AnimationType.MoveFromTo => aAction.CurrentMoveJob,
                _ => null,
            };

            IEnumerator animCoroutine = type switch
            {
                AnimationType.None => null,
                AnimationType.FadeInOutLoop => FadeInOut(aAction),
                AnimationType.ScaleUpDownLoop => ScaleUpDown(aAction),
                AnimationType.MoveFromTo => MoveFromTo(aAction),
                _ => null,
            };

            Action resetAction = type switch
            {
                AnimationType.None => null,
                AnimationType.FadeInOutLoop => () => ResetAlpha(),
                AnimationType.ScaleUpDownLoop => () => ResetScale(),
                AnimationType.MoveFromTo => () => ResetPosition(),
                _ => null,
            };

            if (start)
            {
                if (currentAnimJob != null)
                    return;

                while (!gameObject.activeInHierarchy)
                    await Task.Yield();

#pragma warning disable CS4014 
                StartCoroutine(animCoroutine);
#pragma warning restore CS4014 
                SetCurrentJob(animCoroutine);
            }
            else
            {
                if (currentAnimJob != null)
                    StopCoroutine(currentAnimJob);

                if (animCoroutine != null)
                    StopCoroutine(animCoroutine);

                resetAction?.Invoke();
                SetCurrentJob(null);
            }

            void SetCurrentJob(IEnumerator enumerator)
            {
                _ = type switch
                {
                    AnimationType.None => null,
                    AnimationType.FadeInOutLoop => aAction.CurrentFadeJob = enumerator,
                    AnimationType.ScaleUpDownLoop => aAction.CurrentScaleJob = enumerator,
                    AnimationType.MoveFromTo => aAction.CurrentMoveJob = enumerator,
                    _ => null,
                };
            }
        }

        private IEnumerator FadeInOut(AgentAction aAction)
        {
            yield return new WaitForSeconds(aAction.actionStartDelay);

            bool fade = true;

            while (true)
            {
                float targetAlpha = fade ? aAction.minAlpha : aAction.maxAlpha;

                var tween = DoFade(targetAlpha, aAction.animationDuration);
                tween.OnComplete(() => fade = !fade);

                tween.onUpdate += () => ToggleActivity(tween);

                while (tween.IsActive())
                {
                    if (_isDead) tween.Kill();
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

            while (true)
            {
                float targetScale = scaleDown ? aAction.minScale : aAction.maxScale;

                var tween = transform.DOScale(targetScale, aAction.animationDuration)
                                     .OnComplete(() => scaleDown = !scaleDown);

                while (tween.IsActive())
                {
                    if (_isDead) tween.Kill();
                    yield return null;
                }
            }
        }

        private IEnumerator MoveFromTo(AgentAction aAction)
        {
            yield return new WaitForSeconds(aAction.actionStartDelay);

            Vector3 fromPosition = aAction.fromPosition.position;
            Vector3 toPosition = aAction.toPosition.position;

            while (true)
            {
                transform.position = fromPosition;

                var tween = transform.DOMove(toPosition, aAction.animationDuration * aAction.moveDurationFactor);

                while (tween.IsActive())
                {
                    if (_isDead) tween.Kill();
                    yield return null;
                }

                if (!aAction.loopMove) break;
            }
        }

        private void ToggleActivity(TweenerCore<Color, Color, ColorOptions> tween)
        {
            if (gameObject.activeSelf)
                tween.Play();
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
            _isDead = true;
            StopAllCoroutines();
        }
    }
}
