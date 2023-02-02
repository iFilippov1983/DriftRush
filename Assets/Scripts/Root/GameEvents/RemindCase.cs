using RaceManager.Cars;
using RaceManager.Progress;
using System;
using System.Collections.Generic;
using UniRx;

namespace RaceManager.Root
{
    [Serializable]
    public class RemindCase
    {
        public ProgressConditionType Condition;
        public IReward Reward;
        public List<CarName> CarNames;

        public Subject<ProgressConditionType> OnReminderStart = new Subject<ProgressConditionType>();
        public Subject<ProgressConditionType> OnReminderFinish = new Subject<ProgressConditionType>();

        public RemindCase(ProgressConditionType condition)
        {
            Condition = condition;
        }
    }
}
