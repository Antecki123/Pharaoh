using App.Helpers;
using App.Signals;
using Controllers.Work;
using Models.Economy;
using Models.Work;
using UnityEditor;
using UnityEngine;
using Views.Ui.Buildings;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class FarmersHutView : BuildingView
    {
        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private SupplyModel supplyModel;

        private FarmWorkplace workplace;

        [Inject]
        public void Constructor(SignalBus signalBus, PrefabManager prefabManager, SupplyModel supplyModel)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.supplyModel = supplyModel;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            SetupWorkplace();

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(workplace));
            signalBus.Fire(new WorkplaceSignals.RegisterSupplyTarget(workplace, SupplyType.Workplace));
        }

        public override void DestroyBuilding()
        {
            signalBus.Fire(new WorkplaceSignals.UnregisterWorkplace(workplace));
            signalBus.Fire(new WorkplaceSignals.UnregisterSupplyTarget(workplace));

            base.DestroyBuilding();
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                //var infoPanel = prefabManager.InstantiateUI<HabitationInfoUI>();
                var infoPanel = FindAnyObjectByType<FarmInfoUI>(FindObjectsInactive.Include);
                infoPanel.Init(transform, workplace.WorkplaceModel);
            }
        }

        private void SetupWorkplace()
        {
            var workplaceModel = new WorkplaceModel("Farmers hut", null, null, 5f, 1, 10);
            workplaceModel.AddCommodity(new CommodityModel() { Name = "Wheat", Quantity = 0, MaxQuantity = 50 });
            workplaceModel.AddCommodity(new CommodityModel() { Name = "Linen", Quantity = 0, MaxQuantity = 50 });

            workplace = new FarmWorkplace(prefabManager, supplyModel, workplaceModel, transform.position, EntranceTransform.position);
        }

        private void OnDrawGizmosSelected()
        {
            if (workplace == null)
                return;

            foreach (var crop in workplace.Crops)
            {
                Handles.DrawDottedLine(transform.position, crop.Position, 1f);
                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), crop.Position, Quaternion.identity, 1f, EventType.Repaint);
            }
        }
    }
}