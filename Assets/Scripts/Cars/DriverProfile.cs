using UniRx;

namespace RaceManager.Cars
{
    public class DriverProfile
    {
        public DriverType DriverType;
        public float CarCurrentSpeed;
        public float TrackProgress;
        public int PositionInRace;

        public ReactiveProperty<CarState> CarState;

        public DriverProfile(DriverType driverType)
        {
            DriverType = driverType;
            CarState = new ReactiveProperty<CarState>();
        }
    }
}
