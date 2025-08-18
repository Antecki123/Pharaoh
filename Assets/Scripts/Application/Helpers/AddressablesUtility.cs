using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace App.Helpers
{
    public class AddressablesUtility
    {
        public static UniTask<T> LoadAssetAsync<T>(object key)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            var loadAssetHandle = Addressables.LoadAssetAsync<T>(key);

            loadAssetHandle.Completed += (AsyncOperationHandle<T> completedHandle) =>
            {
                if (completedHandle.OperationException != null)
                    taskCompletionSource.SetException(completedHandle.OperationException);
                else
                    taskCompletionSource.SetResult(completedHandle.Result);
            };
            return taskCompletionSource.Task.AsUniTask();
        }
    }
}