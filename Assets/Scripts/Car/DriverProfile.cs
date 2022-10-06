using RaceManager.Tools;

namespace RaceManager.Cars
{
    public class DriverProfile
    {
        public readonly SubscriptionProperty<CarState> CarState;

        public float CarCurrentSpeed;
        public float TrackProgress;
        public int PositionInRace;

        public DriverProfile()
        {
            CarState = new SubscriptionProperty<CarState>();
        }
    }
}
