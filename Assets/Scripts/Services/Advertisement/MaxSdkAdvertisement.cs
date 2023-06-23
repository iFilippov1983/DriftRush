using RaceManager.Root;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaceManager.Services
{
    public class MaxSdkAdvertisement : MonoBehaviour
    {
        private const string AdType_Interstitial = "interstitial";
        private const string AdType_Rewarded = "rewarded";

        [SerializeField] private string _interstitialUnitId;
        [SerializeField] private string _rewardedUnitId;
        [SerializeField] private string _bannerAdUnitId;
        [SerializeField] private string _sdkKey;
        [SerializeField] private string _userId;

        private int _retryAttempt;

        private bool _playerRewarded;
        private bool _adClicked;
        private bool _adDisplayed;

        private CancellationTokenSource _adsLoadCancellation;

        private AdvertisementPlacement _lastAdsPlacement;

        private Dictionary<AdvertisementPlacement, string> _adsPlacements = new Dictionary<AdvertisementPlacement, string>() 
        {
            { AdvertisementPlacement.RetryingPrevious, "retrying_previous"},
            { AdvertisementPlacement.MenuEnter, "menu_scene_enter"},
            { AdvertisementPlacement.MultiplyRaceScores, "race_scores_multiplication"},
            { AdvertisementPlacement.SpeedupLootboxOpen, "lootbox_open_speedup"}
        };

        public Subject<bool> OnRewardedAdComplete;

        private string LastAdsPlacement => _adsPlacements[_lastAdsPlacement];
        private AnalyticsService AnalyticsService => Singleton<AnalyticsService>.Instance;

        public bool PlayerRewarded => _playerRewarded;
        public bool IsRewardedAdReady => MaxSdk.IsRewardedAdReady(_rewardedUnitId);
        public bool IsInterstitialAdReady => MaxSdk.IsInterstitialReady(_interstitialUnitId);

        #region Unity Functions

        private void Awake()
        {
            _adsLoadCancellation = new CancellationTokenSource();

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
            _adsLoadCancellation?.Cancel();
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
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;

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

            SceneManager.activeSceneChanged += ResetOnSceneChange;
        }

        private void UnsubscribeFromCallbacks()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent -= OnSdkInitialized;

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

            SceneManager.activeSceneChanged -= ResetOnSceneChange;
        }

        private void OnSdkInitialized(MaxSdkBase.SdkConfiguration configuration)
        {
            // AppLovin SDK is initialized, start loading ads
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(_bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerBackgroundColor(_bannerAdUnitId, Color.black);
            MaxSdk.ShowBanner(_bannerAdUnitId);

            MaxSdk.LoadInterstitial(_interstitialUnitId);
            MaxSdk.LoadRewardedAd(_rewardedUnitId);
        }

        private void ResetOnSceneChange(Scene a, Scene b)
        {
            _playerRewarded = false;
            _adClicked = false;
            _adDisplayed = false;
        }

        #endregion

        #region Interstitial Ad Functions

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsService.SendEvent(AnalyticsEventType.Ads_Available, new AdvertisementContext() 
            { 
                AdsType = AdType_Interstitial,
                AdsPlacement = LastAdsPlacement,
                AdsResult = AdvertisementResult.SUCCESS
            });

            _retryAttempt = 0;

            //Debug.Log("[MaxSdkAdvertisment] OnInterstitialLoadedEvent");
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
            int maxPow = 6;
            _retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(maxPow, _retryAttempt));

            Invoke(nameof(ShowInterstitialAd), (float)retryDelay);

            //Debug.Log("[MaxSdkAdvertisment] OnInterstitialLoadFailedEvent");
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsService.SendEvent(AnalyticsEventType.Ads_Started, new AdvertisementContext() 
            { 
                AdsType = AdType_Interstitial,
                AdsPlacement = LastAdsPlacement,
                AdsResult = AdvertisementResult.START
            });

            _adDisplayed = true;

            //Debug.Log("[MaxSdkAdvertisment] OnInterstitialDisplayedEvent");
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsService.SendEvent(AnalyticsEventType.Ads_Started, new AdvertisementContext() 
            { 
                AdsType = AdType_Interstitial,
                AdsPlacement = LastAdsPlacement,
                AdsResult = AdvertisementResult.FAIL
            });

            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            MaxSdk.LoadInterstitial(_interstitialUnitId);

            //Debug.Log("[MaxSdkAdvertisment] OnInterstitialAdFailedToDisplayEvent");
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
        { 
            _adClicked = true;

            //Debug.Log("[MaxSdkAdvertisment] OnInterstitialClickedEvent");
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            string result = _adClicked 
                ? AdvertisementResult.CLICKED 
                : AdvertisementResult.WATCHED;

            AnalyticsService.SendEvent(AnalyticsEventType.Ads_Watched, new AdvertisementContext() 
            { 
                AdsType = AdType_Interstitial,
                AdsPlacement = LastAdsPlacement,
                AdsResult = result
            });

            _adDisplayed = false;

            // Interstitial ad is hidden. Pre-load the next ad.
            MaxSdk.LoadInterstitial(_interstitialUnitId);

            //Debug.Log("[MaxSdkAdvertisment] OnInterstitialHiddenEvent");
        }

        #endregion

        #region Rewarded Ad Functions

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsService.SendEvent(AnalyticsEventType.Ads_Available, new AdvertisementContext()
            {
                AdsType = AdType_Rewarded,
                AdsPlacement = LastAdsPlacement,
                AdsResult = AdvertisementResult.SUCCESS
            });

            _retryAttempt = 0;

            //Debug.Log("[MaxSdkAdvertisment] OnRewardedAdLoadedEvent");
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
            int maxPow = 6;
            _retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(maxPow, _retryAttempt));

            Invoke(nameof(ShowRewardedAd), (float)retryDelay);

            //Debug.Log("[MaxSdkAdvertisment] OnRewardedAdLoadFailedEvent");
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsService.SendEvent(AnalyticsEventType.Ads_Started, new AdvertisementContext()
            {
                AdsType = AdType_Rewarded,
                AdsPlacement = LastAdsPlacement,
                AdsResult = AdvertisementResult.START
            });

            _adDisplayed = true;

            //Debug.Log("[MaxSdkAdvertisment] OnRewardedAdDisplayedEvent");
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsService.SendEvent(AnalyticsEventType.Ads_Started, new AdvertisementContext()
            {
                AdsType = AdType_Rewarded,
                AdsPlacement = LastAdsPlacement,
                AdsResult = AdvertisementResult.FAIL
            });

            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            MaxSdk.LoadRewardedAd(_rewardedUnitId);

            //Debug.Log("[MaxSdkAdvertisment] OnRewardedAdFailedToDisplayEvent");
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _adClicked = true;

            //Debug.Log("[MaxSdkAdvertisment] OnRewardedAdClickedEvent");
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnRewardedAdComplete?.OnNext(_playerRewarded);

            string result = _playerRewarded
                ? AdvertisementResult.WATCHED
                : _adClicked
                ? AdvertisementResult.CLICKED
                : AdvertisementResult.CANCELED;

            AnalyticsService.SendEvent(AnalyticsEventType.Ads_Watched, new AdvertisementContext()
            {
                AdsType = AdType_Rewarded,
                AdsPlacement = LastAdsPlacement,
                AdsResult = result
            });

            _adDisplayed = false;

            // Rewarded ad is hidden. Pre-load the next ad
            MaxSdk.LoadRewardedAd(_rewardedUnitId);

            //Debug.Log("[MaxSdkAdvertisment] OnRewardedAdHiddenEvent");
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // The rewarded ad displayed and the user should receive the reward.
            _playerRewarded = true;

            //Debug.Log("[MaxSdkAdvertisment] OnRewardedAdReceivedRewardEvent");
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
            // ?..

            //Debug.Log("[MaxSdkAdvertisment] OnRewardedAdRevenuePaidEvent");
        }

        #endregion

        #region Public Functions

        public async void ShowInterstitialAd(AdvertisementPlacement placement = AdvertisementPlacement.MenuEnter)
        {
            if(_adDisplayed) return;

            _lastAdsPlacement = placement;
            _playerRewarded = false;
            _adClicked = false;

            if (!IsInterstitialAdReady)
            {
                MaxSdk.LoadInterstitial(_interstitialUnitId);
            }

            while (!IsInterstitialAdReady)
            {
                _adsLoadCancellation.Token.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            MaxSdk.ShowInterstitial(_interstitialUnitId);

            Debug.Log("[MaxSdkAdvertisemet] ShowInterstitial");
        }

        public async void ShowRewardedAd(AdvertisementPlacement placement = AdvertisementPlacement.RetryingPrevious)
        {
            if(_adDisplayed) return;

            _lastAdsPlacement = placement;
            _playerRewarded = false;
            _adClicked = false;

            if (!IsRewardedAdReady)
            {
                MaxSdk.LoadInterstitial(_interstitialUnitId);
            }

            while (!IsRewardedAdReady)
            {
                _adsLoadCancellation.Token.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            MaxSdk.ShowRewardedAd(_rewardedUnitId);

            Debug.Log("[MaxSdkAdvertisement] ShowRewarded");
        }

        #endregion
    }
}



