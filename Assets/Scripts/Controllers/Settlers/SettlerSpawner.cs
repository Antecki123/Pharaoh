using Models.Ai.Pathfinding;
using Models.Settler;
using System;
using UnityEngine;
using Views.Settler;

namespace Controllers.Settler
{
    public struct SettlerViewModelWrapper
    {
        public SettlerView View { get; set; }

        public SettlerModel Model { get; set; }
    }

    public class SettlerSpawner
    {
        public SettlerViewModelWrapper SpawnSettler(Vector3 position)
        {
            var settlerPrefab = Resources.Load<SettlerView>("Prefabs/SettlerView");
            if (settlerPrefab == null)
            {
                Debug.LogError($"Prefab 'settlerPrefab' could not be found in 'Prefabs/SettlerView'. Make sure the path and name are correct.");
                throw new NullReferenceException();
            }

            var settlerView = UnityEngine.Object.Instantiate(settlerPrefab);
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

            return new SettlerViewModelWrapper()
            {
                View = settlerView,
                Model = settlerModel
            };
        }
    }
}