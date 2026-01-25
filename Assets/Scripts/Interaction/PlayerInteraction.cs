using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceScrappers.Interaction
{
    [RequireComponent(typeof(PlayerInteractionDetector), typeof(PlayerPickupSystem))]
    public class PlayerInteraction : NetworkBehaviour
    {
        [SerializeField]
        private InputActionReference interactActionReference;

        private InputAction interactAction;
        private PlayerInteractionDetector detector;
        private PlayerPickupSystem pickupSystem;

        private void Awake()
        {
            detector = GetComponent<PlayerInteractionDetector>();
            pickupSystem = GetComponent<PlayerPickupSystem>();

            if (interactActionReference != null)
            {
                interactAction = interactActionReference.action;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner && interactAction != null)
            {
                interactAction.Enable();
                interactAction.performed += OnInteractPerformed;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
                interactAction.Disable();
            }
        }

        private void OnDisable()
        {
            if (IsOwner && interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
                interactAction.Disable();
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!IsOwner)
                return;

            if (pickupSystem.IsHoldingObject)
            {
                pickupSystem.Drop();
                return;
            }

            if (!detector.HasInteractable)
                return;

            ProcessInteraction(detector.CurrentInteractable);
        }

        private void ProcessInteraction(IInteractable interactable)
        {
            switch (interactable.GetInteractType())
            {
                case IInteractable.InteractType.Pickup:
                    if (interactable is PickupInteractable pickupInteractable)
                    {
                        pickupSystem.TryPickup(pickupInteractable);
                    }
                    break;

                case IInteractable.InteractType.Use:
                    interactable.Interact(NetworkObject);
                    break;

                default:
                    break;
            }
        }
    }
}
