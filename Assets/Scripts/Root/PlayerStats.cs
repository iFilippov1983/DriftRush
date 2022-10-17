using RaceManager.Cars;
using System;

namespace RaceManager.Root
{
    [Serializable]
    public struct PlayerStats
    {
        public int Money;
        public int Gems;

        public DriverProfile DriverProfile;
    }
}
