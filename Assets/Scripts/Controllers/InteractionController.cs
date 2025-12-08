using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

            if (Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask) && !IsUIHit())
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
                if (Physics.Raycast(ray, out RaycastHit hit, 300f, layerMask) && !IsUIHit())
                {
                    if (hit.collider.TryGetComponent(out IInteractable interactable))
                    {
                        interactable.Interact();
                    }
                }
            }
        }

        private bool IsUIHit()
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }
    }

    public interface IInteractable
    {
        public void Highlight(bool state);

        public void Interact();
    }
}