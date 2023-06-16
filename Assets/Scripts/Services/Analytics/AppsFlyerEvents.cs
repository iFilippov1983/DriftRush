using AppsFlyerSDK;
using System.Collections.Generic;
using System.Diagnostics;

namespace RaceManager.Services
{
    public class AppsFlyerEvents
    {
        private const string INSTALL = "af_install";
        private const string SESSION_START = "af_session";

        private bool _hadPreviousLaunches;

        public AppsFlyerEvents(bool hadPreviousLaunches)
        {
            _hadPreviousLaunches = hadPreviousLaunches;
        }

        public void Initialize()
        {
            if (!_hadPreviousLaunches)
            {
                SendEvent_FirstLaunch();
            }

            SendEvent_SessionStart();
        }

        public void SendEvent_FirstLaunch()
        {
            $"[AppsFlyerEvents] FirstLaunch ".Log();
            Dictionary<string, string> eventValues = new Dictionary<string, string>();
            AppsFlyer.sendEvent(INSTALL, eventValues);
        }

        public void SendEvent_SessionStart()
        {
            $"[AppsFlyerEvents] SessionStart ".Log();
            Dictionary<string, string> eventValues = new Dictionary<string, string>();
            AppsFlyer.sendEvent(SESSION_START, eventValues);
        }
    }
}

