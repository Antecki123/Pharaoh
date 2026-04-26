using App.Helpers;
using UnityEngine;
using UnityEngine.Pool;

namespace Controllers.Work
{
    public class WorkerObjectPool<T> where T : MonoBehaviour
    {
        public ObjectPool<T> WorkersPool => workersPool;

        private readonly ObjectPool<T> workersPool;
        private readonly Transform workersContainer;

        private readonly PrefabManager prefabManager;

        public WorkerObjectPool(PrefabManager prefabManager, Transform workersContainer, bool collectionCheck = true,
            int defaultCapacity = 30, int maxSize = 500)
        {
            this.prefabManager = prefabManager;
            this.workersContainer = workersContainer;

            workersPool = new ObjectPool<T>(
            createFunc: Create,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroy,
            collectionCheck: collectionCheck,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize);
        }

        private T Create()
        {
            var worker = prefabManager.Instantiate<T>(typeof(T).Name);
            worker.transform.SetParent(workersContainer);

            return worker;
        }

        private void OnGet(T worker)
        {
            worker.gameObject.SetActive(true);
        }

        private void OnRelease(T worker)
        {
            worker.gameObject.SetActive(false);
        }

        private void OnDestroy(T worker)
        {
            Object.Destroy(worker.gameObject);
        }
    }
}