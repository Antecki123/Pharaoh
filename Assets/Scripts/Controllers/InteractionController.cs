using App.Signals;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Controllers
{
    public class InteractionController : IInitializable, ITickable, IDisposable
    {
        private SignalBus signalBus;

        private Camera mainCamera;
        private IInteractable currentHighlighted;

        private bool interactionBlocked;

        private const int layerMask = 1 << 17;
        private const float raycastDistance = 200f;

        private readonly PointerEventData pointerEventData;
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>(8);

        public InteractionController(SignalBus signalBus)
        {
            this.signalBus = signalBus;

            mainCamera = Camera.main;
            pointerEventData = new PointerEventData(EventSystem.current);
        }

        public void Initialize()
        {
            signalBus.Subscribe<ConstructionSignals.ActivateConstructionMode>(OnConstructionModeChanged);
        }

        public void Tick()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (interactionBlocked)
                return;

            TryGetObjectToSelect();
            TryInteractWithObject();
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<ConstructionSignals.ActivateConstructionMode>(OnConstructionModeChanged);
        }

        private void OnConstructionModeChanged(ConstructionSignals.ActivateConstructionMode signal) => 
            interactionBlocked = signal.State;

        private void TryGetObjectToSelect()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, layerMask) && !IsUIHit())
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (interactable != currentHighlighted)
                    {
                        currentHighlighted?.Highlight(false, default);
                        interactable.Highlight(true, Color.lightGray);
                        currentHighlighted = interactable;
                    }
                }
                else
                {
                    if (currentHighlighted != null)
                    {
                        currentHighlighted.Highlight(false, default);
                        currentHighlighted = null;
                    }
                }
            }
            else
            {
                if (currentHighlighted != null)
                {
                    currentHighlighted.Highlight(false, default);
                    currentHighlighted = null;
                }
            }
        }

        private void TryInteractWithObject()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, layerMask) && !IsUIHit())
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
            pointerEventData.position = Input.mousePosition;
            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            return raycastResults.Count > 0;
        }
    }

    public interface IInteractable
    {
        public void Highlight(bool state, Color color);

        public void Interact();
    }
}