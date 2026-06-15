using Controllers;
using Controllers.Construction;
using Controllers.Work;
using System;
using UnityEngine;

namespace Views.Construction
{
    public class BuildingView : MonoBehaviour, IInteractable, IServiceReceiver
    {
        public BuildingFoundationView BuildingFoundation;
        public BuildingDefinition BuildingDefinition;

        public Guid BuildingId { get; private set; }

        protected bool isPlaced;

        public virtual void PlaceBuilding()
        {
            BuildingId = Guid.NewGuid();
            isPlaced = true;

            if (BuildingFoundation != null)
                BuildingFoundation.GenerateFoundationObjects();
        }

        public virtual void DestroyBuilding()
        {
            isPlaced = false;
        }

        public void Highlight(bool state, Color color)
        {
            if (isPlaced)
            {
                var renderers = GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    renderer.material.color = state ? color : Color.white;
                }
            }
        }

        public virtual void Interact() { }

        public virtual void ReceiveService(IService service) { }
    }
}