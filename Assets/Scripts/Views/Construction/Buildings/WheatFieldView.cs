using App.Signals;
using Models.Economy;
using Models.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Zenject;

namespace Views.Construction
{
    [SelectionBase]
    public class WheatFieldView : BuildingView
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

            var createdMaterial = new CommodityModel() { Name = "Wheat" , Quantity = 3};
            var growthDuration = 20f;
            cropModel = new CropModel("Wheat field", createdMaterial, transform.position, growthDuration);

            cropModel.OnWorkScheduled += PlayTimeline;
            signalBus.Fire(new WorkplaceSignals.RegisterCropField(cropModel));
        }

        public override void DestroyBuilding()
        {
            base.DestroyBuilding();

            cropModel.OnWorkScheduled -= PlayTimeline;
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
            var timeline = timelines.FirstOrDefault(t => t != null && t.name == name);
            if (timeline != null)
            {
                director.playableAsset = timeline;
                director.Play();
            }
            else
            {
                Debug.LogWarning($"Timeline '{name}' not found!");
            }
        }
    }
}