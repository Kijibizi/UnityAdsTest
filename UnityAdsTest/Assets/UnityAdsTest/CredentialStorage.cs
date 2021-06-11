using System;
using UnityEngine;

namespace UnityAdsTest
{
    [CreateAssetMenu]
    public sealed class CredentialStorage : ScriptableObject
    {
        [SerializeField]
        public string GameId;

        [SerializeField]
        public string RewardAdUnitId;

        [SerializeField]
        public string InterstitialAdUnitId;
    }
}