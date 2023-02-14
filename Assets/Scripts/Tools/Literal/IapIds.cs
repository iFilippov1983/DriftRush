namespace RaceManager.Tools
{
    public static class IapIds
    {
        public const string NoAds = "noads";        //nonconsumable
        public const string Gems_80 = "gems_80";    //consumable
        public const string Gems_500 = "gems_500";
        public const string Gems_1200 = "gems_1200";
        public const string Gems_2500 = "gems_2500";
        public const string Gems_6500 = "gems_6500";
        public const string Gems_14000 = "gems_14000";

        public static string[] All => new string[] 
        { 
            NoAds, 
            Gems_80, 
            Gems_500, 
            Gems_1200,
            Gems_2500,
            Gems_6500,
            Gems_14000
        };
    }
}
