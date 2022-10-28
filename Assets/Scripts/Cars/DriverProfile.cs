using RaceManager.Root;
using UniRx;

namespace RaceManager.Cars
{
    public class DriverProfile
    {
        public DriverType DriverType;
        public float CarCurrentSpeed;
        public float TrackProgress;
        public PlayerProfile.PositionInRace PositionInRace;

        public ReactiveProperty<CarState> CarState;

        public DriverProfile(DriverType driverType)
        {
            DriverType = driverType;
            CarState = new ReactiveProperty<CarState>();
        }
    }
}
