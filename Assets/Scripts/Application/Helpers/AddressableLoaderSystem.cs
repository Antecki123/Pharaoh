using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace App.Helpers
{
    public class AddressableLoaderSystem
    {
        public bool EnableLogging { get; set; } = false;

        public bool EnableDetailedWarnings { get; set; } = true;

        public float TimeoutSeconds { get; set; } = 30f;

        public static AddressableLoaderSystem Instance { get; private set; }

        public event Action<string, GameObject> OnAssetLoaded;
        public event Action<string, string> OnAssetLoadFailed;
        public event Action<string> OnAssetReleased;

        private readonly Dictionary<string, CachedAsset> loadedAssets = new();
        private readonly Dictionary<string, UniTask<GameObject>> pendingOperations = new();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static AddressableLoaderSystem()
        {
            Instance = new AddressableLoaderSystem();
        }

        private AddressableLoaderSystem()
        {
            LogMessage("AddressableLoaderSystem initialized");
        }

        /// <summary>
        /// Loads a GameObject by its Addressable key
        /// </summary>
        /// <param name="key">The Addressable key</param>
        /// <param name="forceReload">Whether to force reloading if the asset is already loaded</param>
        /// <returns>The loaded GameObject, or null in case of an error</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string key, bool forceReload = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                LogWarning("Attempted to load asset with null or empty key");
                return null;
            }

            if (!forceReload && loadedAssets.TryGetValue(key, out var cachedAsset))
            {
                if (cachedAsset.IsValid)
                {
                    LogMessage($"Returning cached asset: {key}");
                    cachedAsset.IncrementReferenceCount();
                    return cachedAsset.GameObject;
                }
                else
                {
                    LogWarning($"Cached asset is invalid, removing from cache: {key}");
                    loadedAssets.Remove(key);
                }
            }

            if (pendingOperations.TryGetValue(key, out var pendingOperation))
            {
                LogMessage($"Asset loading already in progress, waiting: {key}");
                return await pendingOperation;
            }

            var loadTask = LoadGameObjectInternalAsync(key);
            pendingOperations[key] = loadTask;

            try
            {
                var result = await loadTask;
                return result;
            }
            finally
            {
                pendingOperations.Remove(key);
            }
        }

        /// <summary>
        /// Releases the asset from memory
        /// </summary>
        /// <param name="key">The key of the asset to release</param>
        public void ReleaseAsset(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                LogWarning("Attempted to release asset with null or empty key");
                return;
            }

            if (loadedAssets.TryGetValue(key, out var cachedAsset))
            {
                cachedAsset.DecrementReferenceCount();

                if (cachedAsset.ReferenceCount <= 0)
                {
                    LogMessage($"Releasing asset from memory: {key}");

                    if (cachedAsset.Handle.IsValid())
                    {
                        Addressables.Release(cachedAsset.Handle);
                    }

                    loadedAssets.Remove(key);
                    OnAssetReleased?.Invoke(key);
                }
                else
                {
                    LogMessage($"Asset still has {cachedAsset.ReferenceCount} references: {key}");
                }
            }
            else
            {
                LogWarning($"Attempted to release asset that was not loaded: {key}");
            }
        }

        private async UniTask<GameObject> LoadGameObjectInternalAsync(string key)
        {
            AsyncOperationHandle<GameObject> handle = default;

            try
            {
                LogMessage($"Starting to load asset: {key}");

                handle = Addressables.LoadAssetAsync<GameObject>(key);

                var result = await handle.WithCancellation(cancellationTokenSource.Token)
                    .Timeout(TimeSpan.FromSeconds(TimeoutSeconds));

                if (handle.Status == AsyncOperationStatus.Succeeded && result != null)
                {
                    var cachedAsset = new CachedAsset(result, handle, key);
                    loadedAssets[key] = cachedAsset;

                    LogMessage($"Successfully loaded asset: {key}");
                    OnAssetLoaded?.Invoke(key, result);

                    return result;
                }
                else
                {
                    var errorMsg = $"Failed to load asset '{key}': {handle.OperationException?.Message ?? "Unknown error"}";
                    LogError(errorMsg);
                    OnAssetLoadFailed?.Invoke(key, errorMsg);

                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                var errorMsg = $"Asset loading cancelled: {key}";
                LogWarning(errorMsg);
                OnAssetLoadFailed?.Invoke(key, errorMsg);
                return null;
            }
            catch (TimeoutException)
            {
                var errorMsg = $"Asset loading timeout ({TimeoutSeconds}s): {key}";
                LogError(errorMsg);
                OnAssetLoadFailed?.Invoke(key, errorMsg);
                return null;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error loading asset '{key}': {ex.Message}";
                LogError(errorMsg);
                OnAssetLoadFailed?.Invoke(key, errorMsg);
                return null;
            }
            finally
            {
                if (handle.IsValid() && handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                }
            }
        }

        public bool IsAssetLoaded(string key)
        {
            return !string.IsNullOrEmpty(key)
                && loadedAssets.TryGetValue(key, out var asset)
                && asset.IsValid;
        }

        public Dictionary<string, AssetInfo> GetLoadedAssetsInfo()
        {
            var info = new Dictionary<string, AssetInfo>();

            foreach (var kvp in loadedAssets)
            {
                info[kvp.Key] = new AssetInfo
                {
                    Key = kvp.Key,
                    IsValid = kvp.Value.IsValid,
                    ReferenceCount = kvp.Value.ReferenceCount,
                    GameObject = kvp.Value.GameObject
                };
            }

            return info;
        }

        public void Dispose()
        {
            foreach (var kvp in loadedAssets)
            {
                if (kvp.Value.Handle.IsValid())
                {
                    Addressables.Release(kvp.Value.Handle);
                }
            }

            loadedAssets.Clear();
            pendingOperations.Clear();

            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        private void LogMessage(string message)
        {
#if UNITY_EDITOR
            if (EnableLogging)
                UnityEngine.Debug.Log($"[AddressableLoader] {message}");
#endif
        }

        private void LogWarning(string message)
        {
#if UNITY_EDITOR
            if (EnableDetailedWarnings)
                UnityEngine.Debug.LogWarning($"[AddressableLoader] {message}");
#endif
        }

        private void LogError(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError($"[AddressableLoader] {message}");
#endif
        }
    }

    public class CachedAsset
    {
        public GameObject GameObject { get; private set; }
        public AsyncOperationHandle<GameObject> Handle { get; private set; }
        public string Key { get; private set; }
        public int ReferenceCount { get; private set; }
        public DateTime LoadedTime { get; private set; }

        public bool IsValid => GameObject != null && Handle.IsValid() && Handle.Status == AsyncOperationStatus.Succeeded;

        public CachedAsset(GameObject gameObject, AsyncOperationHandle<GameObject> handle, string key)
        {
            GameObject = gameObject;
            Handle = handle;
            Key = key;
            ReferenceCount = 1;
            LoadedTime = DateTime.Now;
        }

        public void IncrementReferenceCount() => ReferenceCount++;
        public void DecrementReferenceCount() => ReferenceCount = Mathf.Max(0, ReferenceCount - 1);
    }

    public class AssetInfo
    {
        public string Key { get; set; }
        public bool IsValid { get; set; }
        public int ReferenceCount { get; set; }
        public GameObject GameObject { get; set; }
    }
}