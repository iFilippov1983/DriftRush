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
        public Subject<Unit> MakeReminder = new Subject<Unit>();

        public ReminderCase(ProgressConditionType condition)
        {
            Condition = condition;
        }
    }
}
