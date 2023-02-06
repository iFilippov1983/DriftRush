using RaceManager.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    [RequireComponent(typeof(UIActionAgent))]
    public class UIReminderAgent : MonoBehaviour, ILateInitializable
    {
        [SerializeField] private ProgressConditionType _conditionType;
        [SerializeField] private AgentActionType _actionType;

        [ShowInInspector, ReadOnly]
        private UIActionAgent _actionAgent;

        private GameRemindHandler _remindHandler;
        private GameEvents _gameEvents;

        private Task _currentTask;

        [Inject]
        private void Construct(GameRemindHandler remindHandler, GameEvents gameEvents, Resolver resolver)
        {
            resolver.Add(this);
            _remindHandler = remindHandler;
            _gameEvents = gameEvents;
            _actionAgent = GetComponent<UIActionAgent>();
        }

        public void LateInitialize()
        {
            _remindHandler.Subscribe(_conditionType, MakeReminder, gameObject);
        }

        private void MakeReminder(ReminderCase reminderCase)
        {
            _currentTask = _actionType switch
            {
                AgentActionType.Click => _actionAgent.Click(),
                AgentActionType.InteractableTrue => _actionAgent.ButtonInteractable(true),
                AgentActionType.InteractableFalse => _actionAgent.ButtonInteractable(false),
                AgentActionType.StartAnimation => _actionAgent.StartAnimation(),
                AgentActionType.StopAnimation => _actionAgent.StopAnimation(),
                _ => throw new NotImplementedException()
            };

            _gameEvents.Reminder.OnNext(reminderCase);
        }
    }
}
