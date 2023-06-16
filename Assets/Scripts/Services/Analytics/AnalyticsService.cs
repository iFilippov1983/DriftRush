using UnityEngine;
using RaceManager.Tools;

namespace RaceManager.Services
{
    public class AnalyticsService : MonoBehaviour
    {
        private bool _hadPreviousLaunches;

        private AppsFlyerEvents _appsFlyerEvents;
        private AppMetricaEvents _appMetricaEvents;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            _hadPreviousLaunches = PlayerPrefs.GetInt(TextConstant.PrefsKey_FirstSession, 0) == 1;

            _appsFlyerEvents = new AppsFlyerEvents(_hadPreviousLaunches);
            _appsFlyerEvents.Initialize();

            _appMetricaEvents = new AppMetricaEvents();

            if (!_hadPreviousLaunches)
            {
                _hadPreviousLaunches = true;
                PlayerPrefs.SetInt(TextConstant.PrefsKey_FirstSession, 1);
                PlayerPrefs.Save();
            }
        }

        public void SendEvent(AnalyticsEventType eventType, AdvertisementContext adsContext)
        {
            string type = adsContext.AdsType;
            string placement = adsContext.AdsPlacement;
            string result = adsContext.AdsResult;

            switch (eventType)
            {
                case AnalyticsEventType.Ads_Available:
                    _appMetricaEvents.SendEvent_AdsAvailable(type, placement, result);
                    break;
                case AnalyticsEventType.Ads_Started:
                    _appMetricaEvents.SendEvent_AdsStarted(type, placement, result);
                    break;
                case AnalyticsEventType.Ads_Watched:
                    _appMetricaEvents.SendEvent_AdsWatched(type, placement, result);
                    break;
                default:
                    Debug.LogError($"[AnalyticsService] Incorrect SendEvent method extension use. (AnalyticsEventType: {eventType})");
                    break;
            }
        }

        public void SendEvent(AnalyticsEventType eventType, LevelAnalyticsInfo levelInfo)
        {
            switch (eventType)
            {
                case AnalyticsEventType.Level_Start:
                    _appMetricaEvents.SendEvent_LevelStart
                        (
                            levelInfo.LevelName,
                            levelInfo.LevelCount,
                            levelInfo.LevelRandom
                        );
                    break;
                case AnalyticsEventType.Level_Finish:
                    _appMetricaEvents.SendEvent_LevelFinish
                        (
                            levelInfo.LevelName,
                            levelInfo.LevelCount,
                            levelInfo.LevelRandom,
                            levelInfo.Finished ? "win" : "game_closed",
                            Mathf.RoundToInt(levelInfo.Time)
                        );
                    break;
                default:
                    Debug.LogError($"[AnalyticsService] Incorrect SendEvent method extension use. (AnalyticsEventType: {eventType})");
                    break;
            }
        }
    }
}

