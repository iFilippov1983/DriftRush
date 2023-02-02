using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace RaceManager.Root
{
    public class GameRemindHandler : MonoBehaviour, IInitializable
    {
        [SerializeField] private List<ProgressConditionType> _remindCondition = new List<ProgressConditionType>();

        private List<ProgressConditionType> _conditionsList = new List<ProgressConditionType>();
        [ShowInInspector, ReadOnly]
        private Dictionary<ProgressConditionType, RemindCase> _cases = new Dictionary<ProgressConditionType, RemindCase>();

        private RemindCase _currentCase;
        private IProgressConditionInfo _conditionInfo;

        [Inject]
        private void Construct(IProgressConditionInfo conditionInfo)
        { 
            _conditionInfo = conditionInfo;
        }

        public void Initialize()
        {
            MakeRemindCasesDictionary();
        }

        public void RunReminders()
        {
            if (_cases.Count == 0)
                return;

            var condition = _conditionsList[0];
            _currentCase = _cases[condition];
            _currentCase.OnReminderStart.OnNext(condition);
        }

        public IDisposable Subscribe(ProgressConditionType condition, Action startCallback, Subject<ProgressConditionType> finishRemindSubject)
        {
            RemindCase remindCase;
            if (TryGetCase(condition, out remindCase))
            {
                finishRemindSubject
                    .Subscribe(c => remindCase.OnReminderFinish.OnNext(condition))
                    .AddTo(this);

                return remindCase.OnReminderStart
                    .Take(1)
                    .Subscribe(_ => startCallback?.Invoke());
            }

            return Disposable.Empty;
        }

        private bool TryGetCase(ProgressConditionType condition, out RemindCase remindCase)
            => _cases.TryGetValue(condition, out remindCase) ? true : false;

        private void MakeRemindCasesDictionary()
        {
            foreach (var condition in _remindCondition)
            {
                RemindCase remindCase;
                switch (condition)
                {
                    case ProgressConditionType.CanUpgradeFactors:
                    case ProgressConditionType.CanUpgradeRank:
                    case ProgressConditionType.CanUnlockCar:
                    case ProgressConditionType.HasSpecialIapOffer:
                        remindCase = new RemindCase(condition);
                        break;
                    case ProgressConditionType.None:
                    default:
                        remindCase = null;
                        break;
                }

                if (remindCase == null)
                    continue;

                bool add = remindCase.Condition switch
                {
                    ProgressConditionType.CanUpgradeFactors => true,
                    ProgressConditionType.CanUpgradeRank => _conditionInfo.HasRankUpgradableCars(out remindCase.CarNames),
                    ProgressConditionType.CanUnlockCar => _conditionInfo.HasUlockableCars(out remindCase.CarNames),
                    ProgressConditionType.HasSpecialIapOffer => _conditionInfo.HasIapSpecialOffer(out remindCase.Reward),
                    ProgressConditionType.None => false,
                    _ => false
                };

                if (add)
                {
                    _conditionsList.Add(condition);
                    _cases.Add(condition, remindCase);
                    Debug.Log($"Remind case ADDED for condition => [{condition}]");
                }
            }

            if (_cases.Count == 0)
                return;

            var firstCondition = _conditionsList[0];
            _currentCase = _cases[firstCondition];
            _currentCase.OnReminderFinish.Subscribe(c => StartNextReminder((int)c + 1));
        }

        private void StartNextReminder(int c)
        {
            var nextCondition = (ProgressConditionType)c;
            if (TryGetCase(nextCondition, out RemindCase remindCase))
            {
                remindCase.OnReminderStart.OnNext(nextCondition);
                Debug.Log($"Starting next reminder => [{remindCase.Condition}]");
            }
            else
            {
                Debug.Log($"No reminders with number: {c}");
            }
        }
    }
}
