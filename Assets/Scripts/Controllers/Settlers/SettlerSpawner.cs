using App.Helpers;
using Models.Settler;
using System;
using UnityEngine;
using UnityEngine.Pool;
using Views.Settler;

namespace Controllers.Settler
{
    public class SettlerSpawner
    {
        private PrefabManager prefabManager;
        private SettlersNamesImporter settlersNames;

        private IObjectPool<SettlerView> projectilePool;
        private Transform settlersContainer;
        private int maxPoolSize = 200;

        public SettlerSpawner(PrefabManager prefabManager, SettlersNamesImporter settlersNames)
        {
            this.prefabManager = prefabManager;
            this.settlersNames = settlersNames;

            /*projectilePool = new ObjectPool<SettlerView>(
                () => UnityEngine.Object.Instantiate(simpleProjectile),
                settler => settler.gameObject.SetActive(true),
                settler => settler.gameObject.SetActive(false),
                settler => UnityEngine.Object.Destroy(settler.gameObject),
                true, 50, maxPoolSize);*/

            settlersContainer = new GameObject("SettlersContainer").transform;
        }

        public (SettlerView, SettlerModel) SpawnSettler(Vector3 position, Quaternion rotation)
        {
            var settlerView = prefabManager.Instantiate<SettlerView>("SettlerView");
            var gender = GetRandomGender();
            var name = GetName(gender);

            var settlerDefinition = new SettlerDefinition(Guid.NewGuid(), name, 1, gender);
            var settlerModel = new SettlerModel()
            {
                SettlerDefinition = settlerDefinition,
            };

            settlerView.transform.SetParent(settlersContainer);
            settlerView.Init(settlerModel);

            return (settlerView, settlerModel);
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