using RaceManager.Tools;
using System;
using UniRx;

namespace RaceManager.Cars
{
    public class DriverProfile
    {
        public float CarCurrentSpeed;
        public float TrackProgress;
        public int PositionInRace;

        public ReactiveProperty<CarState> CarState;

        public DriverProfile()
        {
            CarState = new ReactiveProperty<CarState>();
        }
    }
}
