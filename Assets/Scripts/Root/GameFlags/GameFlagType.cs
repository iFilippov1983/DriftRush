namespace RaceManager.Root
{
    public enum GameFlagType
    { 
        None = 0,

        GlobalDeactivation = 1,

        InitialGameStart = 2,
        FirstRace = 3,

        AccelerationStart = 4,
        AccelerationFinish = 5,

        BrakeRequestA = 6,
        BrakeA = 7,

        BrakeRequestB = 40,
        BrakeB = 41,//top

        DriftRequestA = 35,
        DriftReleaseA = 38,
        DriftA = 8,

        DriftRequestB = 36,
        DriftReleaseB = 39,
        DriftB = 37,       

        UpgradeCar = 9,
        TuneCar = 10,
        AdjustCar = 34,

        WinRace_1 = 11,
        WinRace_2 = 12,
        WinRace_3 = 13,
        WinRace_4 = 14,
        WinRace_6 = 15,
        WinRace_8 = 16,

        FirstLootboxOffer = 17,
        FirstLootboxGot = 18,

        TakeProgressRewardOffer = 19,
        ProgressRewardGot = 20,

        LookAtCarsCollectionOffer = 21,
        LookAtCarOffer = 22,
        CarShown = 23,
        CarsCollectionShown = 24,

        LootboxOpenOffer = 25,
        LootboxOpenShown = 26,

        IapLootboxOffer = 27,
        IapLootboxGot = 28,

        WinRace_5 = 29,
        WinRace_7 = 30,
        WinRace_9 = 32,
        WinRace_10 = 33,

        HowToOpenLootboxShown = 31
    }
}
