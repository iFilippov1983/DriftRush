using RaceManager.Cars;
using RaceManager.Progress;
using System;
using System.Collections.Generic;
using UniRx;

namespace RaceManager.Root
{
    [Serializable]
    public class ReminderCase
    {
        public readonly ProgressConditionType Condition;

        public IReward Reward;
        public List<CarName> CarNames;

        public Subject<ReminderCase> OnReminder = new Subject<ReminderCase>();

        public ReminderCase(ProgressConditionType condition)
        {
            Condition = condition;
        }
    }

}
