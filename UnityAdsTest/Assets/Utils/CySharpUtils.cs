using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Utils
{
    public static class CySharpUtils
    {
        public static async UniTask WaitUntilKeyUp(KeyCode keyCode)
        {
            await Observable
                .EveryUpdate()
                .Where(_ => Input.GetKeyUp(keyCode))
                .ToUniTask(useFirstValue: true);
        }

        public static UniTask<T> FirstToTask<T>(this IObservable<T> self, CancellationToken cancellationToken = default)
        {
            return self.ToUniTask(true, cancellationToken);
        }
    }
}