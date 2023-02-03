using RaceManager.Progress;
using RaceManager.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    public class GameRemindHandler : SerializedMonoBehaviour, IInitializable
    {
        private IProgressConditionInfo _conditionInfo;
        private GameEvents _gameEvents;

        [SerializeField]
        private List<ProgressConditionType> _conditionsToRemind = new List<ProgressConditionType>();
        [ShowInInspector, ReadOnly]
        private Dictionary<ProgressConditionType, ReminderCase> _cases = new Dictionary<ProgressConditionType, ReminderCase>();

        [Inject]
        private void Construct(IProgressConditionInfo conditionInfo, GameEvents gameEvents)
        { 
            _conditionInfo = conditionInfo;
            _gameEvents = gameEvents;
        }

        public void Initialize()
        {
            MakeCases();
        }

        public void RunRemindersSequence()
        {
            foreach (var p in _cases)
            {
                ReminderCase reminder = p.Value;
                reminder.OnReminder.OnNext(reminder);
                //Debugger.Log(reminder.Condition.ToString());
                break;
            }
        }

        public bool HasCondition(ProgressConditionType conditionType) => _cases.ContainsKey(conditionType);

        public void Subscribe(ProgressConditionType condition, Action<ReminderCase> startCallback, GameObject sender = null)
        {
            if (HasCondition(condition))
            {
                ReminderCase reminder = _cases[condition];

                reminder.OnReminder
                    .Subscribe(r => startCallback?.Invoke(r))
                    .AddTo(this);

                if (sender != null)
                    $"Reminder Agent subscribed: {sender.name}".Log();
            }
        }

        private void MakeCases()
        {
            foreach (var condition in _conditionsToRemind)
            { 
                ReminderCase reminder = new ReminderCase(condition);

                bool add = condition switch
                {
                    ProgressConditionType.None => false,
                    ProgressConditionType.CanUpgradeFactors => _conditionInfo.CanUpgradeCurrentCarFactors(),
                    ProgressConditionType.CanUpgradeRank => _conditionInfo.HasRankUpgradableCars(out reminder.CarNames),
                    ProgressConditionType.CanUnlockCar => _conditionInfo.HasUlockableCars(out reminder.CarNames),
                    ProgressConditionType.HasSpecialIapOffer => _conditionInfo.HasIapSpecialOffer(out reminder.Reward),
                    _ => false,
                };

                if (add)
                {
                    _cases.Add(condition, reminder);
                    _gameEvents.Reminder
                        .Where(r => r.Condition == reminder.Condition)
                        .Subscribe(r => NextReminder(r.Condition))
                        .AddTo(this);

                    reminder.OnReminder
                    .Subscribe(r => Debugger.Log(r.Condition.ToString()))
                    .AddTo(this);
                }
            }
        }

        private void NextReminder(ProgressConditionType conditionType)
        {
            conditionType++;

            if (HasCondition(conditionType))
            {
                ReminderCase reminderCase = _cases[conditionType];
                reminderCase.OnReminder.OnNext(reminderCase);
            }
        }
    }
}
