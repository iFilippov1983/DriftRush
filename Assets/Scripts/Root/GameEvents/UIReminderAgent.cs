using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    [RequireComponent(typeof(UIActionAgent))]
    public class UIReminderAgent : MonoBehaviour, ILateInitializable
    {
        [SerializeField] private ProgressConditionType _conditionType;

        [ShowInInspector, ReadOnly]
        private UIActionAgent _actionAgent;
        private GameRemindHandler _remindHandler;

        [Inject]
        private void Construct(GameRemindHandler remindHandler, Resolver resolver)
        {
            resolver.Add(this);
            _remindHandler = remindHandler;
            _actionAgent = GetComponent<UIActionAgent>();
        }

        public void LateInitialize()
        {
            _remindHandler.Subscribe(_conditionType, MakeReminder, gameObject);
        }

        private void MakeReminder(ReminderCase reminderCase)
        { 
            
        }
    }
}
