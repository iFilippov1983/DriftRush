﻿using RaceManager.Progress;
using UniRx;
using Zenject;

namespace RaceManager.Root
{
    public class GameEvents
    {
        public Subject<IReward> GotReward = new Subject<IReward>();
        public Subject<string> ButtonPressed = new Subject<string>();
        public Subject<string> Notification = new Subject<string>();
        public Subject<Unit> ScreenTaped = new Subject<Unit>();
        public Subject<Unit> ScreenTapHold = new Subject<Unit>();
        public Subject<Unit> ScreenTapReleased = new Subject<Unit>();
        public Subject<Unit> RaceWin = new Subject<Unit>();

        [Inject]
        private void Cunstruct()
        {
            GotReward = new Subject<IReward>();
            ButtonPressed = new Subject<string>();
            Notification = new Subject<string>();
            ScreenTaped = new Subject<Unit>();
            ScreenTapHold = new Subject<Unit>();
            ScreenTapReleased = new Subject<Unit>();
            RaceWin = new Subject<Unit>();

            Notification.Subscribe(s => DebugLog(s));
        }

        private void DebugLog(string s) => Debugger.Log($"Notification: " + s);
    }
}
