using RaceManager.Progress;
using UniRx;
using Zenject;

namespace RaceManager.Root
{
    public class GameEvents
    {
        public Subject<string> ButtonPressed = new Subject<string>();
        public Subject<IReward> GotReward = new Subject<IReward>();
        public Subject<string> Notification = new Subject<string>();
        public Subject<Unit> ScreenTaped = new Subject<Unit>();
        public Subject<Unit> ScreenTapHold = new Subject<Unit>();
        public Subject<Unit> ScreenTapReleased = new Subject<Unit>();
        public Subject<Unit> RaceWin = new Subject<Unit>();

        [Inject]
        private void Cunstruct()
        {
            ButtonPressed = new Subject<string>();
            GotReward = new Subject<IReward>();
            Notification = new Subject<string>();
            ScreenTaped = new Subject<Unit>();
            ScreenTapHold = new Subject<Unit>();
            ScreenTapReleased = new Subject<Unit>();
            RaceWin = new Subject<Unit>();
        }
    }
}
