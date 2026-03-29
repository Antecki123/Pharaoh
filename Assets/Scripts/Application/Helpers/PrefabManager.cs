using App.Registrators;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace App.Helpers
{
    public class PrefabManager
    {
        private ApplicationRegistrator.SceneContextHolder contextHolder;
        private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        public PrefabManager(ApplicationRegistrator.SceneContextHolder contextHolder)
        {
            this.contextHolder = contextHolder;
        }

        public async UniTask LoadGameObjectsAssets(string assetKey)
        {
            var loadedObject = await AddressableLoaderSystem.Instance.LoadGameObjectAsync(assetKey);

            if (loadedObject != null)
            {
                prefabs.TryAdd(assetKey, loadedObject);
            }
        }

        public T InstantiateWithInject<T>(GameObject prefab) where T : Component
        {
            if (contextHolder.Container == null)
                throw new Exception("SceneContext not set.");

            var gameObject = UnityEngine.Object.Instantiate(prefab);
            contextHolder.Container.InjectGameObject(gameObject);

            return gameObject.GetComponent<T>();
        }

        public T Instantiate<T>(string key) where T : Component
        {
            if (contextHolder.Container == null)
                throw new Exception("SceneContext not set.");

            if (!prefabs.ContainsKey(key))
            {
                UnityEngine.Debug.LogWarning($"Asset {key} has not been loaded. Load asset from addressables first.");
                return null;
            }

            var gameObject = UnityEngine.Object.Instantiate(prefabs[key]);
            contextHolder.Container.InjectGameObject(gameObject);

            return gameObject.GetComponent<T>();
        }

        public GameObject Instantiate(string key)
        {
            if (contextHolder.Container == null)
                throw new Exception("SceneContext not set.");

            if (!prefabs.ContainsKey(key))
            {
                UnityEngine.Debug.LogWarning($"Asset {key} has not been loaded. Load asset from addressables first.");
                return null;
            }

            var gameObject = UnityEngine.Object.Instantiate(prefabs[key]);
            contextHolder.Container.InjectGameObject(gameObject);

            return gameObject;
        }
    }
}