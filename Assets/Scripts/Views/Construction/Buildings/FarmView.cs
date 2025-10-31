using App.Helpers;
using App.Signals;
using Controllers.Construction;
using Controllers.Work;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using Views.Helpers;
using Views.Ui.Buildings;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class FarmView : BuildingView
    {
        [SerializeField] private Transform farmFieldTransform;

        private SignalBus signalBus;
        private PrefabManager prefabManager;
        private SupplyModel supplyModel;

        private FarmWorkplaceNew workplace;

        private BuildingDefinition buildingDefinition;
        private float fieldArea;

        [Inject]
        public void Constructor(SignalBus signalBus, PrefabManager prefabManager, SupplyModel supplyModel)
        {
            this.signalBus = signalBus;
            this.prefabManager = prefabManager;
            this.supplyModel = supplyModel;
        }

        public void Init(BuildingDefinition buildingDefinition, List<Vector3> farmVertices)
        {
            this.buildingDefinition = buildingDefinition;

            fieldArea = MeshBuilder.CalculatePolygonArea(farmVertices);

            farmFieldTransform.GetComponent<MeshFilter>().mesh = MeshBuilder.BuildMeshFromVertices(farmVertices, out var center);
            farmFieldTransform.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            transform.position = new Vector3(center.x, transform.position.y, center.z);

            var coll = GetComponent<MeshCollider>();
            coll.sharedMesh = MeshBuilder.BuildColliderFromVertices(farmVertices);
            coll.convex = false;

            gameObject.layer = 17;
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
                //var infoPanel = prefabManager.InstantiateUI<FarmInfoUI>();
                var infoPanel = FindAnyObjectByType<FarmInfoUI>(FindObjectsInactive.Include);
                infoPanel.Init(transform, workplace.WorkplaceModel);
            }
        }

        private void SetupWorkplace()
        {
            var cropName = buildingDefinition switch
            {
                BuildingDefinition.WheatFarm => CommodityName.Wheat,
                BuildingDefinition.LinenFarm => CommodityName.Linen,
                _ => CommodityName.None
            };

            var storage = new StorageModel(new List<CommodityModel>()
            {
                new CommodityModel() { Name = cropName, Quantity = 0, MaxQuantity = 50 }
            });

            var workplaceModel = new FarmWorkplaceModel(cropName, storage, fieldArea);
            workplace = new FarmWorkplaceNew(prefabManager, supplyModel, workplaceModel, EntranceTransform.position);
        }
    }
}