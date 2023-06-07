using UnityEngine;

public class MaxSdkInitializer : MonoBehaviour
{
    [SerializeField] private string _bannerAdUnitId;
    [SerializeField] private string _sdkKey;
    [SerializeField] private string _userId;

    //---All functionality of this class is duplicated in BootstrapInstaller---//

    void Start()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += InitializeMaxSdk;

        MaxSdk.SetSdkKey(_sdkKey);
        MaxSdk.SetUserId(_userId);
        MaxSdk.InitializeSdk();
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

    private void OnDestroy()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent -= InitializeMaxSdk;
    }
}
