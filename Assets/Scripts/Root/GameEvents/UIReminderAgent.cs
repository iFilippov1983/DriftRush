using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RaceManager.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Logger;

namespace RaceManager.Root
{
    public class UIReminderAgent : MonoBehaviour, ILateInitializable
    {
        [Serializable]
        public class AgentAction
        {
            public ProgressConditionType ConditionType;
            [Space(20)]
            public List<AgentReminderActionType> actions = new List<AgentReminderActionType>();
            [Space]
            [ShowIf("UseAnimations")]
            public List<AnimationType> animationTypes = new List<AnimationType>();
            [ShowIf("NeedToSetAnimations")]
            [Tooltip("Hole animation or animation cycle duration")]
            public float animationDuration = 1f;

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
                actions.Contains(AgentReminderActionType.StartAnimation);
            public bool NeedToSetAnimations =>
                animationTypes.Count != 0 && !animationTypes.Contains(AnimationType.None);
            public bool UseFadeInOut =>
                animationTypes.Contains(AnimationType.FadeInOutLoop) && NeedToSetAnimations;
            public bool UseScaleUpDown =>
                animationTypes.Contains(AnimationType.ScaleUpDownLoop) && NeedToSetAnimations;
            public bool UseMoveFromTo =>
                animationTypes.Contains(AnimationType.MoveFromTo) && NeedToSetAnimations;

            #endregion

            [ShowInInspector, ReadOnly]
            public bool InProcess { get; set; } = false;
            public Dictionary<AnimationType, Tween> AnimTweens { get; set; }
        }

        private Button _button;
        private Image _image;
        private TMP_Text _text;

        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        private float _originalAlpha;

        [SerializeField] private List<AgentAction> _agentActions = new List<AgentAction>();

        private GameRemindHandler _remindHandler;
        private GameEvents _gameEvents;

        public Subject<AnimationType> StopAnimationSubject = new Subject<AnimationType>();

        public AgentType Type { get; private set; }

        [Inject]
        private void Construct(GameRemindHandler remindHandler, GameEvents gameEvents, Resolver resolver)
        {
            resolver.Add(this);
            _remindHandler = remindHandler;
            _gameEvents = gameEvents;

            _originalScale = transform.localScale;
            _originalPosition = transform.position;

            FindProperComponent();
        }

        public void LateInitialize()
        {
            foreach (var aAction in _agentActions)
            {
               MakeSubscriptions(aAction);
            }
        }

        private void MakeSubscriptions(AgentAction aAction)
        {
            foreach (var action in aAction.actions)
            {
                switch (action)
                {
                    case AgentReminderActionType.Click:
                        ClickOnReminder(aAction);
                        break;
                    case AgentReminderActionType.InteractableFalse:
                        ToggleButtonOnReminder(false, aAction);
                        break;
                    case AgentReminderActionType.StartAnimation:
                        ToggleAnimationOnReminder(true, aAction);
                        break;
                }
            }

            Task task = Test();
            
        }

        private Task Test()
        { 
            return Task.CompletedTask;
        }

        private void ClickOnReminder(AgentAction aAction)
        {
            if (Type != AgentType.Button)
            {
                $"You're trying to make a click reminder on UIReminderAgent without Button component. Subscription denied! => [{gameObject.name}]".Log(ColorRed);
                return;
            }

            _remindHandler.Subscribe(aAction.ConditionType, ClickAgent, gameObject);

            _button.onClick.AddListener(() => 
            {
                _gameEvents.ReminderDone.OnNext(aAction.ConditionType);
            });
        }

        private void ToggleButtonOnReminder(bool interactable, AgentAction aAction)
        {
            if (_button == null)
            {
                $"You're trying to make an interactable reminder on UIReminderAgent [{gameObject.name}] without Button component. Subscription denied!".Log(ColorRed);
                return;
            }

            _remindHandler.Subscribe(aAction.ConditionType, () => MakeButtonInteractable(interactable), gameObject);

            _gameEvents.ReminderDone
                .Where(t => t == aAction.ConditionType)
                .Take(1)
                .Subscribe(t =>
                {
                    MakeButtonInteractable(!interactable);
                })
                .AddTo(this);

            if (Type == AgentType.Button)
            {
                _button.onClick.AddListener(() => 
                {
                    _gameEvents.ReminderDone.OnNext(aAction.ConditionType);
                });
            }
        }

        private void ToggleAnimationOnReminder(bool startAnimation, AgentAction aAction)
        {
            bool incorrectAnimationTypes =
                aAction.animationTypes.Count == 0 || aAction.animationTypes.Contains(AnimationType.None);

            if (startAnimation && incorrectAnimationTypes)
            {
                $"You're trying to toggle UIReminderAgent [{gameObject.name}] animation but you haven't assigned correct animation type!".Log(ColorRed);
                return;
            }

            _remindHandler.Subscribe(aAction.ConditionType, () => ToggleAnimation(startAnimation, aAction), gameObject);

            _gameEvents.ReminderDone
                .Where(t => t == aAction.ConditionType)
                .Take(1)
                .Subscribe(t => 
                { 
                    ToggleAnimation(!startAnimation, aAction);
                })
                .AddTo(this);

            if (Type == AgentType.Button)
            {
                _button.onClick.AddListener(() =>
                {
                    _gameEvents.ReminderDone.OnNext(aAction.ConditionType);
                });
            }
        }

        private void ClickAgent()
        {
            _button.onClick?.Invoke();
        }

        private void MakeButtonInteractable(bool interactable)
        { 
            _button.interactable = interactable;
        }

        private void ToggleAnimation(bool startAnimation, AgentAction aAction)
        {
            if (aAction.AnimTweens is null)
                aAction.AnimTweens = new Dictionary<AnimationType, Tween>();

            foreach (var type in aAction.animationTypes)
            {
                if (startAnimation && !AlreadyStartedAnimationTypeOf(type))
                {
                    aAction.InProcess = true;
                    StartAnimation(type, aAction);
                }
                else if(aAction.AnimTweens.ContainsKey(type))
                {
                    aAction.InProcess = false;
                    Tween tween = aAction.AnimTweens[type];
                    tween?.OnKill(() =>
                    {
                        ResetAlpha();
                    });
                    tween?.Kill(true);
                }
            }
        }

        private void StartAnimation(AnimationType type, AgentAction aAction)
        {
            switch (type)
            {
                case AnimationType.None:
                    break;
                case AnimationType.FadeInOutLoop:
                    FadeInOutLoop(aAction);
                    break;
                case AnimationType.ScaleUpDownLoop:
                    ScaleUpDownLoop(aAction);
                    break;
                case AnimationType.MoveFromTo:
                    MoveFromTo(aAction);
                    break;
            }
        }

        private void FadeInOutLoop(AgentAction aAction)
        {
            float targetAlpha = aAction.minAlpha;

            Tween tween = FadeInOut(aAction.minAlpha, aAction.maxAlpha, aAction.animationDuration);
            tween.OnComplete(() => 
            { 
                if(aAction.InProcess)
                    tween.Restart();
            });

            aAction.AnimTweens.Add(AnimationType.FadeInOutLoop, tween);
        }

        private void ResetAlpha()
        {
            switch (Type)
            {
                case AgentType.None:
                    break;
                case AgentType.Button:
                    _button.image.DOFade(_originalAlpha, 0);
                    break;
                case AgentType.Image:
                    _image.DOFade(_originalAlpha, 0);
                    break;
                case AgentType.Text:
                    _text.DOFade(_originalAlpha, 0);
                    break;
            }
        }

        private void ScaleUpDownLoop(AgentAction aAction)
        {
            Debug.Log("ScaleUpDown animation is not implemented");
        }

        private void MoveFromTo(AgentAction aAction)
        {
            Debug.Log("MoveFromTo animation is not implemented");
        }

        private Tween FadeInOut(float minAlpha, float maxAlpha, float duration)
        {
            Sequence sequence = DOTween.Sequence();

            Tween fadeOutTween = Type switch
            {
                AgentType.None => null,
                AgentType.Button => _button.image.DOFade(minAlpha, duration / 2),
                AgentType.Image => _image.DOFade(minAlpha, duration / 2),
                AgentType.Text => _text.DOFade(minAlpha, duration / 2),
                _ => null,
            };

            Tween fadeInTween = Type switch
            {
                AgentType.None => null,
                AgentType.Button => _button.image.DOFade(maxAlpha, duration / 2),
                AgentType.Image => _image.DOFade(maxAlpha, duration / 2),
                AgentType.Text => _text.DOFade(maxAlpha, duration / 2),
                _ => null,
            };

            sequence.Append(fadeOutTween);
            sequence.Append(fadeInTween);

            return sequence;
        }

        private bool AlreadyStartedAnimationTypeOf(AnimationType type)
        {
            foreach (var aAction in _agentActions)
            { 
                if(aAction.InProcess && aAction.AnimTweens.ContainsKey(type))
                    return true;
            }
            return false;
        }

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
            Debug.LogWarning($"UI Reminder Agent [{gameObject.name}] has no porper components to work with!");
        }
    }
}
