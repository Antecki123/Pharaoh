using Controllers;
using UnityEngine;

namespace Views.Construction
{
    public class BuildingView : MonoBehaviour, IInteractable
    {
        protected bool isPlaced;

        public virtual void PlaceBuilding()
        {
            isPlaced = true;
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
                    renderer.material.color = state ? Color.cyan : Color.white;
                }
            }
        }

        public virtual void Interact() { }
    }
}