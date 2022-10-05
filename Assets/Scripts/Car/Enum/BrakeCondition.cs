#pragma warning disable 649
namespace RaceManager.Cars
{
    public enum BrakeCondition
    {
        //for AI
        NeverBrake,                 // the car simply accelerates at full throttle all the time.
        TargetDirectionDifference,  // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.
        TargetDistance,             // the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
                                    // head for a stationary target and come to rest when it arrives there.
    }
}
