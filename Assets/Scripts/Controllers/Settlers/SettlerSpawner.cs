using App.Helpers;
using Cysharp.Threading.Tasks;
using Models.Settler;
using System;
using UnityEngine;
using Views.Settler;

namespace Controllers.Settler
{
    public class SettlerSpawner
    {
        private PrefabManager prefabManager;
        private GameObject settlerPrefab;

        public SettlerSpawner(PrefabManager prefabManager)
        {
            this.prefabManager = prefabManager;

            _ = LoadAssets();
        }

        public (SettlerView, SettlerModel) SpawnSettler(Vector3 position, Quaternion rotation)
        {
            var settlerView = prefabManager.Instantiate<SettlerView>(settlerPrefab);
            var settlerModel = new SettlerModel()
            {
                Id = Guid.NewGuid(),
                Name = "",
                Age = UnityEngine.Random.Range(16, 80),
                MovementSpeed = 2f,
                Gender = SettlerGender.Unknown,
                Profession = SettlerProfession.None
            };

            settlerView.Init(settlerModel);
            settlerView.transform.SetPositionAndRotation(position, rotation);

            return (settlerView, settlerModel);
        }

        private async UniTask LoadAssets()
        {
            settlerPrefab = await AddressablesUtility.LoadAssetAsync<GameObject>("Settler");
        }
    }
}