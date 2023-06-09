using System;
using UniRx;
using UnityEngine;

public class MaxSdkAdvertisement : MonoBehaviour
{
    [SerializeField] private string _adUnitId;
    [SerializeField] private string _bannerAdUnitId;
    [SerializeField] private string _sdkKey;
    [SerializeField] private string _userId;

    private int _retryAttempt;

    private bool _playerRewarded;

    public Subject<bool> OnInterstitialAdDisplay;
    public Subject<bool> OnRewardedAdComplete;

    public bool PlayerRewarded => _playerRewarded;

    #region Unity Functions

    private void Awake()
    {
        OnInterstitialAdDisplay = new Subject<bool>();
        OnRewardedAdComplete = new Subject<bool>();

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        SubscribeToCallbacks();
        InitializeMaxSdk();
    }

    private void OnDestroy()
    {
        UnsubscribeFromCallbacks();
    }

    #endregion

    #region Private Functions

    private void InitializeMaxSdk()
    {
        MaxSdk.SetSdkKey(_sdkKey);
        MaxSdk.SetUserId(_userId);
        MaxSdk.InitializeSdk();
    }

    private void SubscribeToCallbacks()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += InitializeAdBanner;

        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
    }

    private void UnsubscribeFromCallbacks()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent -= InitializeAdBanner;

        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialAdFailedToDisplayEvent;

        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
    }

    private void InitializeAdBanner(MaxSdkBase.SdkConfiguration configuration)
    {
        // AppLovin SDK is initialized, start loading ads
        // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
        // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
        MaxSdk.CreateBanner(_bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerBackgroundColor(_bannerAdUnitId, Color.black);
        MaxSdk.ShowBanner(_bannerAdUnitId);
    }

    #endregion

    #region Interstitial Ad Functions

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        MaxSdk.ShowInterstitial(adUnitId);
        _retryAttempt = 0;
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
        int maxPow = 6;
        _retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(maxPow, _retryAttempt));

        Invoke(nameof(LoadInterstitial), (float)retryDelay);
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        LoadInterstitial();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad.
        // ??
    }

    #endregion

    #region Rewarded Ad Functions

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        MaxSdk.ShowRewardedAd(adUnitId);
        _retryAttempt = 0;

        Debug.Log("[MaxSdkAdvertisment] OnRewardedAdLoadedEvent");
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
        int maxPow = 6;
        _retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(maxPow, _retryAttempt));

        Invoke(nameof(LoadRewardedAd), (float)retryDelay);

        Debug.Log("[MaxSdkAdvertisment] OnRewardedAdLoadedEvent");
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
    {
        Debug.Log("[MaxSdkAdvertisment] OnRewardedAdDisplayedEvent");
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
        LoadRewardedAd();

        Debug.Log("[MaxSdkAdvertisment] OnRewardedAdFailedToDisplayEvent");
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
    {
        Debug.Log("[MaxSdkAdvertisment] OnRewardedAdClickedEvent");
    }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        // ??
        OnRewardedAdComplete?.OnNext(_playerRewarded);

        Debug.Log("[MaxSdkAdvertisment] OnRewardedAdHiddenEvent");
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // The rewarded ad displayed and the user should receive the reward.
        _playerRewarded = true;

        Debug.Log("[MaxSdkAdvertisment] OnRewardedAdReceivedRewardEvent");
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Ad revenue paid. Use this callback to track user revenue.
        // ??

        Debug.Log("[MaxSdkAdvertisment] OnRewardedAdRevenuePaidEvent");
    }

    #endregion

    #region Public Functions

    public void LoadInterstitial()
    {
        _playerRewarded = false;
        MaxSdk.LoadInterstitial(_adUnitId);

        Debug.Log("[MaxSdkAdvertisemetn] LoadInterstitial");
    }

    public void LoadRewardedAd()
    {
        _playerRewarded = false;
        MaxSdk.LoadRewardedAd(_adUnitId);

        Debug.Log("[MaxSdkAdvertisemetn] LoadRewarded");
    }

    #endregion
}
