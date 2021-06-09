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
        Text _interstitialAdsCompletionText;

        /// <summary>
        /// UI text to show the number of rewarded video ads displayed to user in this session.
        /// </summary>
        [SerializeField]
        Text _rewardedVideoAdsCounterText;

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

        /// <summary>
        /// The number of rewarded video ads displayed to user in this session.
        /// </summary>
        int _rewardedVideoAdsCount;

        void Start()
        {
            // Don't start the app if credentials aren't given
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
                .Subscribe(_ => InitializeMoPubSDK().Forget())
                .AddTo(this);

            _loadInterstitialAdButton
                .OnClickAsObservable()
                .Subscribe(_ => RequestInterstitialAd().Forget())
                .AddTo(this);

            _showInterstitialAdButton
                .OnClickAsObservable()
                .Subscribe(_ => ShowInterstitialAd().Forget())
                .AddTo(this);

            _loadRewardedVideoAdButton
                .OnClickAsObservable()
                .Subscribe(_ => RequestRewardedVideoAd().Forget())
                .AddTo(this);

            _showRewardedVideoAdButton
                .OnClickAsObservable()
                .Subscribe(_ => ShowRewardedVideoAd().Forget())
                .AddTo(this);
        }

        /// <summary>
        /// Initializes MoPub SDK.
        /// </summary>
        async UniTask InitializeMoPubSDK()
        {
            Debug.Log("Initializing MoPub SDK...");

            // no double tap
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

            MoPub.LoadInterstitialPluginsForAdUnits(new[]
            {
                _credentials.InterstitialAdUnitId,
            });

            MoPub.LoadRewardedVideoPluginsForAdUnits(new[]
            {
                _credentials.RewardAdUnitId,
            });

            // let user load ads now
            _loadInterstitialAdButton.interactable = true;
            _loadRewardedVideoAdButton.interactable = true;

            Debug.Log("Finished initializing MoPub SDK");
        }

        /// <summary>
        /// Request an interstitial ad (to be shown later)
        /// </summary>
        async UniTask RequestInterstitialAd()
        {
            using (var cts = new CancellationTokenSource().AddTo(this))
            {
                Debug.Log("Loading interstitial ad...");

                // no double tap
                _loadInterstitialAdButton.interactable = false;

                MoPub.RequestInterstitialAd(_credentials.InterstitialAdUnitId);

                // wait for either "loaded" or "failed load" callback event
                // TODO check against id for when multiple id's are loaded
                var (_, errorOrSuccess) = await UniTask.WhenAny(new[]
                {
                    MoPubUtils.OnInterstitialLoadedAsObservable().Select(_ => (string) null).FirstToTask(cts.Token),
                    MoPubUtils.OnInterstitialLoadFailedAsObservable().Select(r => r.Error).FirstToTask(cts.Token),
                });

                cts.Cancel();

                if (errorOrSuccess is { } error) // if load failed
                {
                    // user can retry
                    _loadInterstitialAdButton.interactable = true;

                    Debug.LogError($"Failed loading interstitial ad: {error}");
                    return;
                }

                // let user show ads now
                _showInterstitialAdButton.interactable = true;

                Debug.Log("Finished loading interstitial ad");
            }
        }

        /// <summary>
        /// Show the requested interstitial ad to user.
        /// </summary>
        async UniTask ShowInterstitialAd()
        {
            using (var cts = new CancellationTokenSource().AddTo(this))
            using (var disposable = new CompositeDisposable().AddTo(this))
            {
                Debug.Log("Showing interstitial ad...");

                // no double taps
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
                    MoPubUtils.OnInterstitialExpiredAsObservable().Select(_ => "expired").FirstToTask(cts.Token),
                });

                cts.Cancel();

                _interstitialAdsCompletionText.text = state;

                // user can load ads again
                _loadInterstitialAdButton.interactable = true;

                Debug.Log($"Finished showing interstitial ad: \"{state}\"");
            }
        }

        /// <summary>
        /// Request a rewarded video ad (to be shown later)
        /// </summary>
        async UniTask RequestRewardedVideoAd()
        {
            using (var cts = new CancellationTokenSource().AddTo(this))
            {
                Debug.Log("Loading rewarded video ad...");

                // no double tap
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

                cts.Cancel();

                if (errorOrSuccess is { } error) // load failed
                {
                    // user can retry
                    _loadRewardedVideoAdButton.interactable = true;

                    Debug.LogError($"Failed loading rewarded video ad: {error}");
                    return;
                }

                // user can show ads now
                _showRewardedVideoAdButton.interactable = true;

                Debug.Log("Finished loading rewarded video ad");
            }
        }

        /// <summary>
        /// Show the requested rewarded video ad to user.
        /// </summary>
        async UniTask ShowRewardedVideoAd()
        {
            using (var cts = new CancellationTokenSource().AddTo(this))
            {
                Debug.Log("Showing rewarded video ad...");

                // no double taps
                _showRewardedVideoAdButton.interactable = false;

                MoPub.ShowRewardedVideo(_credentials.RewardAdUnitId);

                // TODO check against id for when multiple id's are loaded
                await MoPubUtils.OnRewardedVideoShownAsObservable().FirstToTask(cts.Token);

                Debug.Log("Ad shown");

                // wait for the first "coming back to app" callback event
                // TODO check against id for when multiple id's are loaded
                var (_, (success, state)) = await UniTask.WhenAny(new[]
                {
                    MoPubUtils.OnRewardedVideoClosedAsObservable().Select(_ => (true, "closed")).FirstToTask(cts.Token),
                    MoPubUtils.OnRewardedVideoExpiredAsObservable().Select(_ => (false, "expired")).FirstToTask(cts.Token),
                    MoPubUtils.OnRewardedVideoFailedToPlayAsObservable().Select(_ => (false, "failed")).FirstToTask(cts.Token),
                });

                cts.Cancel();

                if (success)
                {
                    _rewardedVideoAdsCount += 1;
                    _rewardedVideoAdsCounterText.text = $"{_rewardedVideoAdsCount}";
                }
                else
                {
                    Debug.LogError($"Failed to show rewarded video ad: {state}");
                }

                // you can load ads again
                _loadRewardedVideoAdButton.interactable = true;

                Debug.Log($"Finished showing rewarded video ad: \"{success}\"");
            }
        }
    }
}