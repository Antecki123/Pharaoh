using App.Helpers;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Zenject;

namespace App.Registrators
{
    public class ApplicationInitializer : IInitializable
    {
        private PrefabManager prefabManager;

        public ApplicationInitializer(PrefabManager prefabManager)
        {
            this.prefabManager = prefabManager;
        }

        public void Initialize()
        {
            _ = LoadAssets();
        }

        private async UniTask LoadAssets()
        {
            var assetsKeys = new List<string>()
            {
                "SettlerView",
                "CarrierView",
                "ServiceAgentView",
                "Bazaar",
                "Bakery",
                "Cottage",
                "WheatFarm",
                "LinenFarm",
                "Pasture",
                "Windmill",
                "House",
                "Granary",
                "Warehouse",
                "Well",
                "Brewery",
                "WeavingMill",
                "Tavern",

                // UI ELEMENTS
                "ProcessingWorkplaceTooltipUI",
                "HabitationTooltipUI",
                "StorageTooltipUI",
                "DistributionPointTooltipUI",
                "FarmTooltipUI"
            };

            foreach (var key in assetsKeys)
            {
                await prefabManager.LoadGameObjectsAssets(key);
            }
        }
    }
}