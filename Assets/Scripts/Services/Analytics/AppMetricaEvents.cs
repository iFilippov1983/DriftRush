using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Services
{
    public class AppMetricaEvents
    {
        private const string ADS_AVAILABLE = "video_ads_available";
        private const string ADS_STARTED = "video_ads_started";
        private const string ADS_WATCHED = "video_ads_watch";

        private const string LEVEL_START = "level_start";
        private const string LEVEL_FINISH = "level_finish";

        private const string PAYMENT_SUCCEED = "payment_succeed";

        // ad_type      str     interstitial, rewarded                      Тип рекламы
        // placement    str     ad_on_replay, get_health, get_shop_entry    Откуда игрок попал в просмотр рекламы (плейсмент внутри игры)
        // result       str     success, not_available etc.                 Результат запроса видеорекламы (если ролик тогов к показу - success, если не готов - not_available)
        // connection   bool	1, 0 (true, false)	                        Состояние интернет соединения, его наличие или отсутствие (на момент совершения события)
        public void SendEvent_AdsAvailable(string ad_type, string placement, string result)
        {
            bool connection = Application.internetReachability != NetworkReachability.NotReachable;
            Dictionary<string, object> eventValues = new Dictionary<string, object>()
            {
                { "ad_type", ad_type },
                { "placement", placement },
                { "result", result },
                { "connection", connection ? 1 : 0 }
            };
            AppMetrica.Instance.ReportEvent(ADS_AVAILABLE, eventValues);
            //AppMetrica.Instance.SendEventsBuffer();

#if UNITY_EDITOR
            Debug.Log($"<color=blue>[AppMetricaEvents] Ads Available Event: ad_type = {ad_type}, placement = {placement}, result = {result}, connection = {(connection ? 1 : 0)}</color>");
#endif
        }

        // ad_type      str     interstitial, rewarded                      Тип рекламы
        // placement    str     ad_on_replay, get_health, get_shop_entry    Откуда игрок попал в просмотр рекламы(плейсмент внутри игры)
        // result       str     start etc.                                  Результат начала просмотра ролика (start - начало просмотра ролика, cancel - отмена просмотра, если ролик был готов, но у пользователя он не прогрузился и юзер закрыл окно)
        // connection   bool	1, 0 (true, false)	                        Состояние интернет соединения, его наличие или отсутствие (на момент совершения события)
        public void SendEvent_AdsStarted(string ad_type, string placement, string result)
        {
            bool connection = Application.internetReachability != NetworkReachability.NotReachable;
            Dictionary<string, object> eventValues = new Dictionary<string, object>()
            {
                { "ad_type", ad_type },
                { "placement", placement },
                { "result", result },
                { "connection", connection ? 1 : 0 }
            };
            AppMetrica.Instance.ReportEvent(ADS_STARTED, eventValues);
            //AppMetrica.Instance.SendEventsBuffer();

#if UNITY_EDITOR
            Debug.Log($"<color=blue>[AppMetricaEvents] Ads Started Event: ad_type = {ad_type}, placement = {placement}, result = {result}, connection = {(connection ? 1 : 0)}</color>");
#endif
        }

        // ad_type      str     interstitial, rewarded                              Тип рекламы
        // placement    str     ad_on_replay, get_health, get_shop_entry            Откуда игрок попал в просмотр рекламы(плейсмент внутри игры)
        // result       str     obligatory: watched, optional: clicked, canceled    Результат просмотра видеорекламы. Досмотрел до конца и получил награду, кликнул на рекламу (и тоже получил награду), 
        //                                                                          либо закрыл приложение и не досмтрел до конца (т.е. не получил награду). На один просмотр рекламы должно отправляться только одно событие
        // connection   bool	1, 0 (true, false)	                                Состояние интернет соединения, его наличие или отсутствие (на момент совершения события)
        public void SendEvent_AdsWatched(string ad_type, string placement, string result)
        {
            bool connection = Application.internetReachability != NetworkReachability.NotReachable;
            Dictionary<string, object> eventValues = new Dictionary<string, object>()
            {
                { "ad_type", ad_type },
                { "placement", placement },
                { "result", result },
                { "connection", connection ? 1 : 0 }
            };
            AppMetrica.Instance.ReportEvent(ADS_WATCHED, eventValues);
            //AppMetrica.Instance.SendEventsBuffer();

#if UNITY_EDITOR
            Debug.Log($"<color=blue>[AppMetricaEvents] Ads Watched Event: ad_type = {ad_type}, placement = {placement}, result = {result}, connection = {(connection ? 1 : 0)}</color>");
#endif
        }

        public void SendEvent_LevelStart(string levelName, int levelCount, bool levelRandom)
        {
            Dictionary<string, object> eventValues = new Dictionary<string, object>()
            {
                { "level_name", levelName },
                { "level_count", levelCount },
                { "level_random", levelRandom ? 1 : 0 }
            };
            AppMetrica.Instance.ReportEvent(LEVEL_START, eventValues);
            AppMetrica.Instance.SendEventsBuffer();

#if UNITY_EDITOR
            Debug.Log($"<color=blue>[AppMetrica] Level Start Event: level_name = {levelName}, level_count = {levelCount}, level_random = {(levelRandom ? 1 : 0)}</color>");
#endif
        }

        public void SendEvent_LevelFinish(string levelName, int levelCount, bool levelRandom, string result, int completeTime)
        {
            Dictionary<string, object> eventValues = new Dictionary<string, object>()
            {
                { "level_name", levelName },
                { "level_count", levelCount },
                { "level_random", levelRandom ? 1 : 0 },
                { "result", result },
                { "time", completeTime },
            };
            AppMetrica.Instance.ReportEvent(LEVEL_FINISH, eventValues);
            AppMetrica.Instance.SendEventsBuffer();

#if UNITY_EDITOR
            Debug.Log($"<color=blue>[AppMetrica] Level Finish Event: level_name = {levelName}, level_count = {levelCount}, level_random = {(levelRandom ? 1 : 0)}, result = {result}, time = {completeTime}</color>");
#endif
        }
    }
}
