using System;

namespace RaceManager.Cars
{
    [Serializable]
    public enum CarName
    { 
        ToyotaSupra = 0,    //Common
        FordMustang = 1,    //Common
        Ferrari488 = 2,     //Uncommon
        NissanSilvia = 3,   //Uncommon
        Porche911 = 4,      //Rare
        TeslaRoadster = 5,  //Rare
        DodgeTRX = 6,       //Legendary
        DodgeCharger = 7,
    }
}
