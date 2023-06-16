namespace RaceManager.Services
{
    public enum AnalyticsEventType
    {
        None = 0,

        Session_Initial,
        Session_Start,
        Session_Finish,

        Level_Start,
        Level_Finish,

        Ads_Available,
        Ads_Started,
        Ads_Watched,

        Payment_Succeed
    }
}

