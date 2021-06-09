using System;
using UniRx;

namespace UnityAdsTest
{
    public static class MoPubUtils
    {
        public static IObservable<string> OnSdkInitializedAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnSdkInitializedEvent += h,
                h => MoPubManager.OnSdkInitializedEvent -= h);
        }

        public static IObservable<string> OnInterstitialLoadedAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnInterstitialLoadedEvent += h,
                h => MoPubManager.OnInterstitialLoadedEvent -= h);
        }

        public static IObservable<(string Id, string Error)> OnInterstitialLoadFailedAsObservable()
        {
            return Observable.FromEvent<Action<string, string>, (string, string)>(
                h => (a1, a2) => h((a1, a2)),
                h => MoPubManager.OnInterstitialFailedEvent += h,
                h => MoPubManager.OnInterstitialFailedEvent -= h);
        }

        public static IObservable<string> OnInterstitialDismissedAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnInterstitialDismissedEvent += h,
                h => MoPubManager.OnInterstitialDismissedEvent -= h);
        }

        public static IObservable<string> OnInterstitialExpiredAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnInterstitialExpiredEvent += h,
                h => MoPubManager.OnInterstitialExpiredEvent -= h);
        }

        public static IObservable<string> OnInterstitialClickedAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnInterstitialClickedEvent += h,
                h => MoPubManager.OnInterstitialClickedEvent -= h);
        }

        public static IObservable<string> OnInterstitialShownAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnInterstitialShownEvent += h,
                h => MoPubManager.OnInterstitialShownEvent -= h);
        }

        public static IObservable<string> OnRewardedVideoLoadedAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnRewardedVideoLoadedEvent += h,
                h => MoPubManager.OnRewardedVideoLoadedEvent -= h);
        }

        public static IObservable<(string Id, string Error)> OnRewardedVideoLoadFailedAsObservable()
        {
            return Observable.FromEvent<Action<string, string>, (string, string)>(
                h => (id, error) => h((id, error)),
                h => MoPubManager.OnRewardedVideoFailedEvent += h,
                h => MoPubManager.OnRewardedVideoFailedEvent -= h);
        }

        public static IObservable<string> OnRewardedVideoShownAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnRewardedVideoShownEvent += h,
                h => MoPubManager.OnRewardedVideoShownEvent -= h);
        }

        public static IObservable<string> OnRewardedVideoClickedAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnRewardedVideoClickedEvent += h,
                h => MoPubManager.OnRewardedVideoClickedEvent -= h);
        }

        public static IObservable<string> OnRewardedVideoClosedAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnRewardedVideoClosedEvent += h,
                h => MoPubManager.OnRewardedVideoClosedEvent -= h);
        }

        public static IObservable<string> OnRewardedVideoExpiredAsObservable()
        {
            return Observable.FromEvent<string>(
                h => MoPubManager.OnRewardedVideoExpiredEvent += h,
                h => MoPubManager.OnRewardedVideoExpiredEvent -= h);
        }

        public static IObservable<(string Id, string Error)> OnRewardedVideoFailedToPlayAsObservable()
        {
            return Observable.FromEvent<Action<string, string>, (string, string)>(
                h => (id, error) => h((id, error)),
                h => MoPubManager.OnRewardedVideoFailedToPlayEvent += h,
                h => MoPubManager.OnRewardedVideoFailedToPlayEvent -= h);
        }

        public static IObservable<(string Id, string Key, float Value)> OnRewardedVideoReceivedRewardAsObservable()
        {
            return Observable.FromEvent<Action<string, string, float>, (string, string, float)>(
                h => (s, s1, arg3) => h((s, s1, arg3)),
                h => MoPubManager.OnRewardedVideoReceivedRewardEvent += h,
                h => MoPubManager.OnRewardedVideoReceivedRewardEvent -= h);
        }
    }
}