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

        private SettlersNamesImporter settlersNames;

        public SettlerSpawner(PrefabManager prefabManager, SettlersNamesImporter settlersNames)
        {
            this.prefabManager = prefabManager;
            this.settlersNames = settlersNames;

            _ = LoadAssets();
        }

        public (SettlerView, SettlerModel) SpawnSettler(Vector3 position, Quaternion rotation)
        {
            var settlerView = prefabManager.Instantiate<SettlerView>(settlerPrefab);
            var gender = GetRandomGender();
            var name = GetName(gender);

            var settlerDefinition = new SettlerDefinition(Guid.NewGuid(), name, 1, gender);
            var settlerModel = new SettlerModel()
            {
                SettlerDefinition = settlerDefinition,
                Profession = SettlerProfession.None
            };

            settlerView.Init(settlerModel);

            return (settlerView, settlerModel);
        }

        private async UniTask LoadAssets()
        {
            settlerPrefab = await AddressablesUtility.LoadAssetAsync<GameObject>("Settler");
        }

        private SettlerGender GetRandomGender()
        {
            var values = (SettlerGender[])Enum.GetValues(typeof(SettlerGender));
            return values[UnityEngine.Random.Range(1, values.Length)];
        }

        private string GetName(SettlerGender gender)
        {
            return gender switch
            {
                SettlerGender.Male => settlersNames.MaleNames[UnityEngine.Random.Range(0, settlersNames.MaleNames.Count)],
                SettlerGender.Female => settlersNames.FemaleNames[UnityEngine.Random.Range(0, settlersNames.FemaleNames.Count)],
                _ => throw new NotImplementedException()
            };
        }
    }
}