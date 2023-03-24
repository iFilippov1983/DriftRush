using RaceManager.Progress;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<ProgressConditionType, (ReminderCase Case, bool Reminded)> _cases = new Dictionary<ProgressConditionType, (ReminderCase Case, bool Reminded)>();

        [Inject]
        private void Construct(IProgressConditionInfo conditionInfo, GameEvents gameEvents)
        { 
            _conditionInfo = conditionInfo;
            _gameEvents = gameEvents;
        }

        public void Initialize()
        {
            MakeReminderCases();
        }

        public void RunRemindersSequence()
        {
            foreach (var p in _cases)
            {
                ReminderCase reminder = p.Value.Case;
                reminder.MakeReminder.OnNext();
                return;
            }
        }

        public bool HasCondition(ProgressConditionType condition) => _cases.ContainsKey(condition);

        public int MaxConditionValue()
        { 
            int max = 0;
            foreach (var c in _cases) 
            { 
                if ((int)c.Key > max)
                    max = (int)c.Key;
            }
            return max;
        }

        public void Subscribe(ProgressConditionType condition, Action makeReminderCallback, GameObject agentGo = null)
        {
            if (HasCondition(condition))
            {
                ReminderCase reminder = _cases[condition].Case;

                reminder.MakeReminder
                    .Take(1)
                    .Subscribe(_ => makeReminderCallback?.Invoke())
                    .AddTo(this);

                _gameEvents.ReminderDone
                    .Where(t => t == condition)
                    .Subscribe(t => 
                    { 
                        NextReminder(t);
                    })
                    .AddTo(this);

                if (agentGo != null)
                    Debug.Log($"Reminder Agent [{agentGo.name}] subscribed");
            }
        }

        private void MakeReminderCases()
        {
            foreach (var condition in _conditionsToRemind)
            {
                bool add = condition switch
                {
                    ProgressConditionType.None => false,
                    ProgressConditionType.CanUpgradeFactors => _conditionInfo.CanUpgradeCurrentCarFactors(),
                    ProgressConditionType.CanUpgradeRank => _conditionInfo.HasRankUpgradableCars(),
                    ProgressConditionType.CanUnlockCar => _conditionInfo.HasUlockableCars(),
                    ProgressConditionType.HasSpecialIapOffer => _conditionInfo.HasIapSpecialOffer(),
                    _ => false,
                };

                if (add)
                {
                    ReminderCase rCase = new ReminderCase(condition);
                    _cases.Add(condition, (Case: rCase, Reminded: false));
                }
            }
        }

        private void NextReminder(ProgressConditionType conditionType)
        {
            if (HasCondition(conditionType))
            {
                var value = _cases[conditionType];
                value.Reminded = true;
                _cases[conditionType] = value;
            }

            if (TryGetConditionNotReminded(out ReminderCase rCase))
            {
                rCase.MakeReminder.OnNext();

                Debug.Log($"Next reminder condition: {rCase.Condition}");
            }
        }

        public bool TryGetConditionNotReminded(out ReminderCase rCase)
        {
            foreach (var p in _cases)
            {
                if (p.Value.Reminded) continue;

                rCase = p.Value.Case;
                return true;
            }

            rCase = null;
            return false;
        }
    }
}
