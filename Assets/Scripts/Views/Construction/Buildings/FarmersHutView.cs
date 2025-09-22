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
        private FarmWorkplace workplace;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            SetupWorkplace();

            signalBus.Fire(new WorkplaceSignals.RegisterWorkplace(workplace));
        }

        public override void DestroyBuilding()
        {
            base.DestroyBuilding();
            signalBus.Fire(new WorkplaceSignals.UnregisterWorkplace(workplace));
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

            var wheatdQuantity = Random.Range(0, 500);
            var linenQuantity = Random.Range(0, 500);
            workplaceModel.AddCommodity(new CommodityModel() { Name = "Wheat", Quantity = wheatdQuantity, MaxQuantity = 500 });
            workplaceModel.AddCommodity(new CommodityModel() { Name = "Linen", Quantity = linenQuantity, MaxQuantity = 500 });

            var workersCount = Random.Range(0, 10);
            for (int i = 0; i < workersCount; i++)
                workplaceModel.AddWorker(new Object());

            workplace = new FarmWorkplace(workplaceModel, transform.position);
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