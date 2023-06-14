using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using RaceManager.Tools;

public class AnalyticsService : MonoBehaviour
{
    private bool _hadPreviousLaunches;

    private AppsFlyerEvents _appsFlyerEvents;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        
    }

    public void Start()
    {
        _hadPreviousLaunches = PlayerPrefs.GetInt(TextConstant.PrefsKey_FirstSession, 0) == 1;

        _appsFlyerEvents = new AppsFlyerEvents(_hadPreviousLaunches);
        _appsFlyerEvents.Initialize();

        if (!_hadPreviousLaunches)
        {
            _hadPreviousLaunches = true;
            PlayerPrefs.SetInt(TextConstant.PrefsKey_FirstSession, 1);
            PlayerPrefs.Save();
        }
    }
}
