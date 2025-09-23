using App.Signals;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class LinenFieldView : BuildingView
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private List<TimelineAsset> timelines;

        private SignalBus signalBus;
        private CropModel cropModel;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();

            var createdMaterial = new CommodityModel() { Name = "Linen" , Quantity = 2};
            var growthDuration = 40f;
            cropModel = new CropModel("Linen field", createdMaterial, transform.position, growthDuration);

            cropModel.OnWorkScheduled += PlayTimeline;
            signalBus.Fire(new WorkplaceSignals.RegisterCropField(cropModel));
        }

        public override void DestroyBuilding()
        {
            base.DestroyBuilding();

            signalBus.Fire(new WorkplaceSignals.UnregisterCropField(cropModel));
        }

        public override void Interact()
        {
            base.Interact();

            if (isPlaced)
            {
                //var infoPanel = prefabManager.InstantiateUI<HabitationInfoUI>();
                var infoPanel = FindAnyObjectByType<CropInfoUI>(FindObjectsInactive.Include);
                infoPanel.Init(transform, cropModel);
            }
        }

        private void PlayTimeline(string name)
        {

        }
    }
}