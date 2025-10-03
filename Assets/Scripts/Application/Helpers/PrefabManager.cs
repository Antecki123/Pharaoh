using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace App.Helpers
{
    public class PrefabManager
    {
        private Context context;
        private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        public PrefabManager(Context context)
        {
            this.context = context;
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
            var gameObject = Object.Instantiate(prefab);
            context.Container.InjectGameObject(gameObject);
            return gameObject.GetComponent<T>();
        }

        public T Instantiate<T>(string key) where T : Component
        {
            if (!prefabs.ContainsKey(key))
            {
                Debug.LogWarning($"Asset {key} has not been loaded. Load asset from addressables first.");
                return null;
            }

            var gameObject = Object.Instantiate(prefabs[key]);
            context.Container.InjectGameObject(gameObject);
            return gameObject.GetComponent<T>();
        }

        public GameObject Instantiate(string key)
        {
            if (!prefabs.ContainsKey(key))
            {
                Debug.LogWarning($"Asset {key} has not been loaded. Load asset from addressables first.");
                return null;
            }
            var gameObject = Object.Instantiate(prefabs[key]);
            context.Container.InjectGameObject(gameObject);
            return gameObject;
        }
    }
}