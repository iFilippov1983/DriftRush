using RaceManager.Root;
using UniRx;

namespace RaceManager.Cars
{
    public class DriverProfile
    {
        public DriverType DriverType;
        public float CarCurrentSpeed;
        public float TrackProgress;
        public float DistanceFromStart;
        //public PositionInRace PositionInRace = PositionInRace.Sixth;
        public int PositionInRace = 6;

        public ReactiveProperty<CarState> CarState;

        public DriverProfile(DriverType driverType)
        {
            DriverType = driverType;
            CarState = new ReactiveProperty<CarState>();
        }
    }
}
