using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UnityAdsTest
{
    public class Main : MonoBehaviour
    {
        [SerializeField]
        CredentialStorage _credentials;

        [SerializeField]
        Text _completionStateText; // for interstitial ad's result

        [Header("Buttons")]
        [SerializeField]
        Button _initializeMoPubButton;

        [SerializeField]
        Button _loadInterstitialAdButton;

        [SerializeField]
        Button _showInterstitialAdButton;

        [SerializeField]
        Button _loadRewardedVideoAdButton;

        [SerializeField]
        Button _showRewardedVideoAdButton;

        void Start()
        {
            if (_credentials == null || !_credentials)
            {
                throw new InvalidOperationException("No credentials set");
            }

            _initializeMoPubButton.interactable = true;

            // don't let user load/show ads until SDK is initialized
            _loadInterstitialAdButton.interactable = false;
            _showInterstitialAdButton.interactable = false;
            _loadRewardedVideoAdButton.interactable = false;
            _showRewardedVideoAdButton.interactable = false;

            _initializeMoPubButton
                .OnClickAsObservable()
                .Subscribe(_ => InitializeMoPub().Forget())
                .AddTo(this);

            _loadInterstitialAdButton
                .OnClickAsObservable()
                .Subscribe(_ => LoadInterstitialAd().Forget())
                .AddTo(this);

            _showInterstitialAdButton
                .OnClickAsObservable()
                .Subscribe(_ => ShowInterstitialAd().Forget())
                .AddTo(this);

            _loadRewardedVideoAdButton
                .OnClickAsObservable()
                .Subscribe(_ => LoadRewardedVideoAd().Forget())
                .AddTo(this);

            _showRewardedVideoAdButton
                .OnClickAsObservable()
                .Subscribe(_ => ShowRewardedVideoAd().Forget())
                .AddTo(this);
        }

        async UniTask InitializeMoPub()
        {
            Debug.Log("Initializing MoPub SDK...");

            // don't let user re-initialize SDK
            _initializeMoPubButton.interactable = false;

            // copied from UnityAdsNetworkConfig.cs
            var options = new MoPubBase.MediatedNetwork
            {
                AdapterConfigurationClassName =
                    Application.platform == RuntimePlatform.Android
                        ? "com.mopub.mobileads.UnityAdsAdapterConfiguration"
                        : "UnityAdsAdapterConfiguration",

                NetworkConfiguration = new Dictionary<string, string>
                {
                    {"gameId", _credentials.GameId},
                },

                MediationSettings = new Dictionary<string, object>(),
                MoPubRequestOptions = new Dictionary<string, string>(),
            };

            // copied from MoPubManager.cs
            var sdkConfiguration = new MoPubBase.SdkConfiguration
            {
                AdUnitId = _credentials.InterstitialAdUnitId,
                MediatedNetworks = new[] {options},
                LogLevel = MoPubBase.LogLevel.Info,
            };

            MoPub.InitializeSdk(sdkConfiguration);
            MoPub.ReportApplicationOpen();
            MoPub.EnableLocationSupport(false);

            // wait for the initialization callback event
            await MoPubUtils.OnSdkInitializedAsObservable().FirstToTask();

            // let user load ads now
            _loadInterstitialAdButton.interactable = true;
            _loadRewardedVideoAdButton.interactable = true;

            Debug.Log("Finished initializing MoPub SDK");
        }

        async UniTask LoadInterstitialAd()
        {
            using (var cts = new CancellationTokenSource())
            using (Disposable.CreateWithState(cts, c => c.Cancel())) // will unsubscribe from all callback events
            {
                Debug.Log("Loading interstitial ad...");

                // don't let user double tap
                _loadInterstitialAdButton.interactable = false;

                MoPub.LoadInterstitialPluginsForAdUnits(new[]
                {
                    _credentials.InterstitialAdUnitId,
                });

                MoPub.RequestInterstitialAd(_credentials.InterstitialAdUnitId);

                // wait for either "loaded" or "failed load" callback event
                // TODO check against id for when multiple id's are loaded
                var (_, errorOrSuccess) = await UniTask.WhenAny(new[]
                {
                    MoPubUtils.OnInterstitialLoadedAsObservable().Select(_ => (string) null).FirstToTask(cts.Token),
                    MoPubUtils.OnInterstitialLoadFailedAsObservable().Select(r => r.Error).FirstToTask(cts.Token),
                });

                // you can tap now
                _loadInterstitialAdButton.interactable = true;

                if (errorOrSuccess is { } error) // load failed
                {
                    Debug.LogError($"Failed loading interstitial ad: {error}");
                    return;
                }

                // load success

                // let user show ads now
                _showInterstitialAdButton.interactable = true;

                Debug.Log("Finished loading interstitial ad");
            }
        }

        async UniTask ShowInterstitialAd()
        {
            using (var cts = new CancellationTokenSource())
            using (Disposable.CreateWithState(cts, c => c.Cancel())) // will unsubscribe from all callback events
            {
                Debug.Log("Showing interstitial ad...");

                // prevent double taps
                _showInterstitialAdButton.interactable = false;

                MoPub.ShowInterstitialAd(_credentials.InterstitialAdUnitId);

                // TODO check against id for when multiple id's are loaded
                await MoPubUtils.OnInterstitialShownAsObservable().FirstToTask(cts.Token);

                Debug.Log("Ad shown");

                // wait for the first "coming back to app" callback event
                // TODO check against id for when multiple id's are loaded
                var (_, state) = await UniTask.WhenAny(new[]
                {
                    MoPubUtils.OnInterstitialDismissedAsObservable().Select(_ => "dismissed").FirstToTask(cts.Token),
                    MoPubUtils.OnInterstitialClickedAsObservable().Select(_ => "clicked").FirstToTask(cts.Token),
                    MoPubUtils.OnInterstitialExpiredAsObservable().Select(_ => "expired").FirstToTask(cts.Token),
                });

                _completionStateText.text = state;

                // you can tap now
                _showInterstitialAdButton.interactable = true;

                Debug.Log($"Finished showing interstitial ad: \"{state}\"");
            }
        }

        async UniTask LoadRewardedVideoAd()
        {
            using (var cts = new CancellationTokenSource())
            using (Disposable.CreateWithState(cts, c => c.Cancel())) // will unsubscribe from all callback events
            {
                Debug.Log("Loading rewarded video ad...");

                // don't let user re-load ads
                _loadRewardedVideoAdButton.interactable = false;

                MoPub.LoadRewardedVideoPluginsForAdUnits(new[]
                {
                    _credentials.RewardAdUnitId,
                });

                MoPub.RequestRewardedVideo(_credentials.RewardAdUnitId);

                // wait for either "loaded" or "failed load" callback event
                // TODO check against id for when multiple id's are loaded
                var (_, errorOrSuccess) = await UniTask.WhenAny(new[]
                {
                    MoPubUtils.OnRewardedVideoLoadedAsObservable().Select(_ => (string) null).FirstToTask(cts.Token),
                    MoPubUtils.OnRewardedVideoLoadFailedAsObservable().Select(r => r.Error).FirstToTask(cts.Token),
                });

                if (errorOrSuccess is { } error) // load failed
                {
                    Debug.LogError($"Failed loading rewarded video ad: {error}");
                    return;
                }

                // load success

                // let user show ads now
                _showRewardedVideoAdButton.interactable = true;

                Debug.Log("Finished loading rewarded video ad");
            }
        }

        async UniTask ShowRewardedVideoAd()
        {
            using (var cts = new CancellationTokenSource())
            using (Disposable.CreateWithState(cts, c => c.Cancel())) // will unsubscribe from all callback events
            {
                Debug.Log("Showing rewarded video ad...");

                // prevent double taps
                _showInterstitialAdButton.interactable = false;

                MoPub.ShowRewardedVideo(_credentials.InterstitialAdUnitId);

                // TODO check against id for when multiple id's are loaded
                await MoPubUtils.OnRewardedVideoShownAsObservable().FirstToTask(cts.Token);

                Debug.Log("Ad shown");

                // wait for the first "coming back to app" callback event
                // TODO check against id for when multiple id's are loaded
                var (_, state) = await UniTask.WhenAny(new[]
                {
                    MoPubUtils.OnInterstitialDismissedAsObservable().Select(_ => "dismissed").FirstToTask(cts.Token),
                    MoPubUtils.OnInterstitialClickedAsObservable().Select(_ => "clicked").FirstToTask(cts.Token),
                    MoPubUtils.OnInterstitialExpiredAsObservable().Select(_ => "expired").FirstToTask(cts.Token),
                });

                _completionStateText.text = state;

                // you can tap now
                _showInterstitialAdButton.interactable = true;

                Debug.Log($"Finished showing rewarded video ad: \"{state}\"");
            }
        }
    }
}