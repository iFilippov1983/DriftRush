using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System;
using System.Threading.Tasks;
using System.Globalization;
using RaceManager.Root;
using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Race;
using SaveData = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;

namespace RaceManager.Infrastructure
{
    public class BootstrapInstaller : BaseInstaller
    {
        [SerializeField] private string _bannerAdUnitId;
        [SerializeField] private string _sdkKey;
        [SerializeField] private string _userId;

        private Action<SaveData> _aotAction;

        public override void InstallBindings()
        {
            AotEnsureObjects();
        }

        public override void Start()
        {
            base.Start();

            MaxSdkCallbacks.OnSdkInitializedEvent += InitializeMaxSdk;
            TaskScheduler.UnobservedTaskException += HandleTaskException;

            Application.targetFrameRate = 60;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            MaxSdk.SetSdkKey(_sdkKey);
            MaxSdk.SetUserId(_userId);
            MaxSdk.InitializeSdk();

            Loader.Load(Loader.Scene.MenuScene);
        }

        private void InitializeMaxSdk(MaxSdkBase.SdkConfiguration configuration)
        {
            // AppLovin SDK is initialized, start loading ads
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(_bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(_bannerAdUnitId, Color.black);
            MaxSdk.ShowBanner(_bannerAdUnitId);
        }

        private void AotEnsureObjects()
        {
            AotHelper.Ensure(() => _aotAction.Invoke(null));

            AotHelper.EnsureType<SaveData>();
            AotHelper.EnsureType<JObject>();
            AotHelper.EnsureType<SaveManager>();
            AotHelper.EnsureType<SaveManager.SaveAction>();
            AotHelper.EnsureType<SaveManager.LoadAction>();

            AotHelper.EnsureList<int>();
            AotHelper.EnsureList<float>();
            AotHelper.EnsureList<string>();

            AotHelper.EnsureList<Action<SaveData>>();
            AotHelper.EnsureList<CarProfile>();
            AotHelper.EnsureList<Lootbox>();
            AotHelper.EnsureList<TutorialSteps.TutorialStep>();

            AotHelper.EnsureList<GameFlagType>();
            AotHelper.EnsureList<LevelName>();

            AotHelper.EnsureDictionary<string, JObject>();
            AotHelper.EnsureDictionary<int, ProgressStep>();
            AotHelper.EnsureDictionary<GameUnitType, GameProgressScheme.ExchangeRateData>();
        }

        private void HandleTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.LogError(e.Exception);
        }

        private void OnDestroy()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent -= InitializeMaxSdk;
            TaskScheduler.UnobservedTaskException -= HandleTaskException;
        }
    }
}
