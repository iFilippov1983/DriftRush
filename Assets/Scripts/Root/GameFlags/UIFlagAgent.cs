using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
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
using System.Linq;

namespace RaceManager.Root
{
    public class UIFlagAgent : MonoBehaviour, IInitializable, ILateInitializable
    {
        public enum AgentActionType
        {
            Click,
            InteractableTrue,
            InteractableFalse,
            StartAnimation,
            StopAnimation
        }

        public enum ReminderActionType
        { 
            Click,
            StartAnimation
        }

        public enum AgentType
        {
            None,
            Button,
            Image,
            Text
        }

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
            public float animationStartDelay = 0f;

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

            //Doesn't have technical sense. Only for debug.
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
            //

            #endregion
        }

        [Serializable]
        public class ReminderAction : AgentAction
        {
            public ProgressConditionType Condition;
            //[Space(20)]
            public List<ReminderActionType> remindActions = new List<ReminderActionType>();
            //[Space]
            //[ShowIf("UseAnimations")]
            //public List<AnimationType> animationTypes = new List<AnimationType>();

            //[ShowIf("UseAnimations")]
            //public float animationDuration = 1f;
            //[ShowIf("UseAnimations")]
            //public float animationStartDelay = 0f;

            //[Header("Fade In/Out Settings")]
            //[ShowIf("UseFadeInOut")]
            //[Range(0f, 1f)]
            //public float minAlpha;
            //[ShowIf("UseFadeInOut")]
            //[Range(0f, 1f)]
            //public float maxAlpha = 1f;

            //[Header("Scale Up/Down Setings")]
            //[ShowIf("UseScaleUpDown")]
            //[Range(0f, 10f)]
            //public float minScale = 1f;
            //[ShowIf("UseScaleUpDown")]
            //[Range(0f, 10f)]
            //public float maxScale = 1.05f;

            //[Header("Move From/To Settings")]
            //[ShowIf("UseMoveFromTo")]
            //public Transform fromPosition;
            //[ShowIf("UseMoveFromTo")]
            //public Transform toPosition;
            //[ShowIf("UseMoveFromTo")]
            //[Range(0.01f, 10f)]
            //public float moveDurationFactor = 1f;
            //[ShowIf("UseMoveFromTo")]
            //public bool loopMove;

            public Subject<ProgressConditionType> OnObjectDisabled = new Subject<ProgressConditionType>();

            //#region ShowIf Editor Properties

            //public bool UseAnimations =>
            //    actions.Contains(ReminderActionType.StartAnimation);
            //public bool UseFadeInOut =>
            //    animationTypes.Contains(AnimationType.FadeInOutLoop);
            //public bool UseScaleUpDown =>
            //    animationTypes.Contains(AnimationType.ScaleUpDownLoop);
            //public bool UseMoveFromTo =>
            //    animationTypes.Contains(AnimationType.MoveFromTo);

            //#endregion

            //#region Debug fields

            //////Doesn't have technical sense. Only for debug.
            //////[Header("Only for debug")]
            //////[ShowInInspector, ReadOnly]
            ////[HideInInspector]
            ////public IEnumerator CurrentFadeJob;
            //////[ShowInInspector, ReadOnly]
            ////[HideInInspector]
            ////public IEnumerator CurrentScaleJob;
            //////[ShowInInspector, ReadOnly]
            ////[HideInInspector]
            ////public IEnumerator CurrentMoveJob;
            //////

            //#endregion
        }

        [Header("Flags Actions")]
        [SerializeField] private List<AgentAction> _agentActions = new List<AgentAction>();
        [SerializeField] private bool useAsReminder;
        [Header("Reminder Actions")]
        [ShowIf("useAsReminder")]
        [SerializeField] private List<ReminderAction> _reminderActions = new List<ReminderAction>();

        private GameFlagsHandler _flagsHandler;
        private GameRemindHandler _remindHandler;
        private Button _button;
        private Image _image;
        private TMP_Text _text;

        private Vector3 _originalScale = default;
        private Vector3 _originalPosition = default;
        private float _originalAlpha = default;

        private bool _isDead = false;

        public AgentType Type { get; private set; }

        [Inject]
        private void Construct(GameFlagsHandler flagsHandler, GameRemindHandler remindHandler, Resolver resolver)
        {
            resolver.Add(this);
            _flagsHandler = flagsHandler;
            _remindHandler = remindHandler;
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

        public void LateInitialize()
        {
            foreach (var rAction in _reminderActions)
            {
                foreach (var action in rAction.remindActions)
                {
                    switch (action)
                    {
                        case ReminderActionType.Click:
                            ClickOnRemind(rAction);
                            break;
                        case ReminderActionType.StartAnimation:
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
                $"UI Flag Agent [{gameObject.name}] has no any Button component, but you whant to click it!".Log(Logger.ColorRed);
                return;
            }

            if (TimeForAction(aAction.key))
            { 
                if(IsLastKey(aAction.key))
                    ClickAgent();

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, ClickAgent)
                .AddTo(this);

            $"ClickOnFlag subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]".Log(ColorBlue);
        }

        private void ClickOnRemind(ReminderAction rAction)
        {
            if (Type != AgentType.Button)
            {
                $"UI Flag Agent [{gameObject.name}] has no any Button component, but you whant to click it!".Log(Logger.ColorRed);
                return;
            }

            _remindHandler
                .Subscribe(rAction.Condition, ClickAgent, rAction.OnObjectDisabled)
                .AddTo(this);

            $"ClickOnRemind subscribed => Condition: [{rAction.Condition}] => Object: [{gameObject.name}]".Log(ColorGreen);
        }

        private void InteractableOnFlag(bool interactable, AgentAction aAction)
        {
            if (Type != AgentType.Button)
            {
                $"UI Flag Agent [{gameObject.name}] has no any Button component, but you whant to set its Interactable to True!".Log(Logger.ColorRed);
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

            $"InteractableOnFlag ({interactable}) subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]".Log(ColorBlue);
        }

        private void ToggleAnimationOnFlag(bool start, AgentAction aAction)
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
                    ToggleAnimationsStatus(start, aAction);

                return;
            }

            _flagsHandler
                .Subscribe(aAction.key, () => ToggleAnimationsStatus(start, aAction))
                .AddTo(this);

            $"ToggleAnimationOnFlag ({start}) subscribed => Key: [{aAction.key}] => Object: [{gameObject.name}]".Log(ColorBlue);
        }

        private void ToggleAnimationOnReminder(bool start, ReminderAction rAction)
        {
            bool incorrectAnimationTypes =
                rAction.animationTypes.Count == 0 || rAction.animationTypes.Contains(AnimationType.None);

            if (start && incorrectAnimationTypes)
            {
                $"You're trying to animate UI Flag Agent [{gameObject.name}] but you haven't assigned correct animation type!".Log(ColorRed);
                return;
            }

            _remindHandler
                .Subscribe(rAction.Condition, () => ToggleAnimationsStatus(start, rAction), rAction.OnObjectDisabled)
                .AddTo(this);

            rAction.OnObjectDisabled
                .Subscribe(c => ToggleAnimationsStatus(!start, rAction))
                .AddTo(this);

            $"ToggleAnimationOnReminder ({start}) subscribed => Condition: [{rAction.Condition}] => Object: [{gameObject.name}]".Log(ColorGreen);
        }

        private void ClickAgent()
        {
            _button.onClick?.Invoke();
        }

        private void MakeButtonInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        private void ToggleAnimationsStatus(bool start, AgentAction aAction)
        {
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

#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
                StartCoroutine(animCoroutine);
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
                SetCurrentJob(animCoroutine);
            }
            else
            {
                if (currentAnimJob != null)
                    StopCoroutine(currentAnimJob);

                if (animCoroutine != null)
                    StopCoroutine(animCoroutine);

                resetAction?.Invoke();
                currentAnimJob = null;
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
            yield return new WaitForSeconds(aAction.animationStartDelay);

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
            yield return new WaitForSeconds(aAction.animationStartDelay);

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
            yield return new WaitForSeconds(aAction.animationStartDelay);

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

        private void OnDisable()
        {
            foreach (var rAction in _reminderActions)
            {
                rAction.OnObjectDisabled.OnNext(rAction.Condition);
            }
        }

        private void OnDestroy()
        {
            _isDead = true;
            StopAllCoroutines();
        }
    }
}
