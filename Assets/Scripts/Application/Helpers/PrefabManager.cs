using UnityEngine;
using Zenject;

namespace App.Helpers
{
    public class PrefabManager
    {
        private Context context;

        public PrefabManager(Context context)
        {
            this.context = context;
        }

        public T Instantiate<T>(GameObject prefab) where T : Component
        {
            var gameObject = Object.Instantiate(prefab);
            context.Container.InjectGameObject(gameObject);
            return gameObject.GetComponent<T>();
        }

        public GameObject InstantiateGO(GameObject prefab)
        {
            var gameObject = Object.Instantiate(prefab);
            context.Container.InjectGameObject(gameObject);
            return gameObject;
        }

    }
}