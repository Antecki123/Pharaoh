using Controllers;
using UnityEngine;

namespace Views.Construction
{
    public class BuildingView : MonoBehaviour, IInteractable
    {
        public BuildingFoundationView BuildingFoundation;

        protected bool isPlaced;

        public virtual void PlaceBuilding()
        {
            isPlaced = true;

            if (BuildingFoundation != null)
                BuildingFoundation.GenerateFoundationObjects();
        }

        public virtual void DestroyBuilding()
        {
            isPlaced = false;
        }

        public void Highlight(bool state)
        {
            if (isPlaced)
            {
                var renderers = GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    renderer.material.color = state ? Color.lightGray : Color.white;
                }
            }
        }

        public virtual void Interact() { }
    }
}