using System;

namespace RaceManager.Cars
{
    [Serializable]
    public enum CarState
    {
        InShed,
        OnTrack,
        Stuck,
        Finished,
        GotHit
    }
}
