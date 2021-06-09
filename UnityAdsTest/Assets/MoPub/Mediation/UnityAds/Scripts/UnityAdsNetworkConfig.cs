#if mopub_manager
using UnityEngine;

public class UnityAdsNetworkConfig : MoPubNetworkConfig
{
    public override string AdapterConfigurationClassName
    {
        get { return Application.platform == RuntimePlatform.Android
                  ? "com.mopub.mobileads.UnityAdsAdapterConfiguration"
                  : "UnityAdsAdapterConfiguration"; }
    }

    [Tooltip("Enter your Game ID to be used to initialize the Unity Ads SDK.")]
    [Config.Optional]
    public PlatformSpecificString gameId;
}
#endif
