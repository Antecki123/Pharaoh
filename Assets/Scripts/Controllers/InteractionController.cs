using UnityEngine;
using Zenject;

namespace Controllers
{
    public class InteractionController : IInitializable, ITickable
    {
        private Camera mainCamera;
        private IInteractable currentHighlighted;

        public void Initialize()
        {
            mainCamera = Camera.main;
        }

        public void Tick()
        {
            if (mainCamera == null)
                return;

            TryGetObjectToSelect();
            TryInteractWithObject();
        }

        private void TryGetObjectToSelect()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var layerMask = 1 << 17;

            if (Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (interactable != currentHighlighted)
                    {
                        currentHighlighted?.Highlight(false);
                        interactable.Highlight(true);
                        currentHighlighted = interactable;
                    }
                }
                else
                {
                    if (currentHighlighted != null)
                    {
                        currentHighlighted.Highlight(false);
                        currentHighlighted = null;
                    }
                }
            }
            else
            {
                if (currentHighlighted != null)
                {
                    currentHighlighted.Highlight(false);
                    currentHighlighted = null;
                }
            }
        }

        private void TryInteractWithObject()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var layerMask = 1 << 17;

            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask))
                {
                    if (hit.collider.TryGetComponent(out IInteractable interactable))
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }

    public interface IInteractable
    {
        public void Highlight(bool state);

        public void Interact();
    }
}