using SpaceScrappers.Debugging;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceScrappers.Interaction
{
    public class PlayerInteraction : NetworkBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactionDistance = 2f;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private InputActionReference interactActionReference;

        [SerializeField] private Transform holdSpotPosition;
        [SerializeField] private float throwForce = 10f;
        [SerializeField] private float throwDistanceFromCamera = 1.5f;

        private NetworkObject heldObject;
        private Rigidbody heldObjectRigidbody;

        private InputAction interactAction;

        private IInteractable currentInteractable;

        private void Awake()
        {
            if (interactActionReference != null)
            {
                interactAction = interactActionReference.action;
            }
        }

        private void Start()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                if (interactAction != null)
                {
                    interactAction.Enable();
                    interactAction.performed += OnInteract;
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && interactAction != null)
            {
                interactAction.performed -= OnInteract;
                interactAction.Disable();
            }
        }

        private void OnDisable()
        {
            if (IsOwner && interactAction != null)
            {
                interactAction.performed -= OnInteract;
                interactAction.Disable();
            }
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            CheckForInteractable();
            UpdateHeldObjectPosition();
        }

        private void UpdateHeldObjectPosition()
        {
            if (heldObject != null && holdSpotPosition != null)
            {
                heldObject.transform.position = holdSpotPosition.position;
                heldObject.transform.rotation = holdSpotPosition.rotation;
            }
        }

        private void CheckForInteractable()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (currentInteractable == null)
                    {
                        currentInteractable = interactable;
                        currentInteractable.OnHoverStart();
                    }
                    else if (currentInteractable != interactable)
                    {
                        currentInteractable.OnHoverEnd();
                        currentInteractable = interactable;
                        currentInteractable.OnHoverStart();
                    }
                    else
                    {
                        // Same interactable, do nothing
                    }
                }
                else
                {
                    UnsetCurrentInteractable();
                }
            }
            else
            {
                UnsetCurrentInteractable();
            }
        }

        private void UnsetCurrentInteractable()
        {
            if (currentInteractable != null)
            {
                if (currentInteractable is Object obj && obj != null)
                {
                    currentInteractable.OnHoverEnd();
                }

                currentInteractable = null;
            }
        }

        private void Interact()
        {
            if (!IsOwner)
                return;

            if (heldObject != null)
            {
                DropObject();
                return;
            }

            if (currentInteractable == null) return;

            switch (currentInteractable.GetInteractType())
            {
                case IInteractable.InteractType.Pickup:
                    if (currentInteractable is PickupInteractable pickupInteractable)
                    {
                        PickupObject(pickupInteractable);
                    }

                    break;
                case IInteractable.InteractType.Use:
                    UseObject();
                    break;
                case IInteractable.InteractType.Examine:
                    ExamineObject();
                    break;
                case IInteractable.InteractType.Talk:
                    TalkToObject();
                    break;
                case IInteractable.InteractType.Inspect:
                    InspectObject();
                    break;
                default:
                    break;
            }
        }
        private void PickupObject(PickupInteractable pickupInteractable)
        {
            var networkObject = pickupInteractable.NetworkObject;
            pickupInteractable.Interact(NetworkObject);
            if (networkObject == null)
            {
                Debug.LogError("Item not a network object");
                return;
            }

            if (!networkObject.IsSpawned)
            {
                Debug.LogError("NetworkObject is not spawned, cannot pickup");
                return;
            }

            heldObject = networkObject;
            heldObjectRigidbody = networkObject.GetComponent<Rigidbody>();

            PickupObjectServerRpc(networkObject.NetworkObjectId);
            pickupInteractable.enabled = false;
        }

        [Rpc(SendTo.Server)]
        private void PickupObjectServerRpc(ulong objectId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
            {

                networkObject.ChangeOwnership(OwnerClientId);

                Rigidbody rb = networkObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                PickupInteractable pickup = networkObject.GetComponent<PickupInteractable>();
                if (pickup != null)
                {
                    pickup.SetCollidersEnabled(false);
                }

                networkObject.TrySetParent(NetworkObject);

                PickupObjectClientRpc(objectId);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PickupObjectClientRpc(ulong objectId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
            {
                Rigidbody rb = networkObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                PickupInteractable pickup = networkObject.GetComponent<PickupInteractable>();
                if (pickup != null)
                {
                    pickup.SetCollidersEnabled(false);
                }

                networkObject.TrySetParent(NetworkObject);
            }
        }

        private void UseObject()
        {
            currentInteractable.Interact(NetworkObject);
        }

        private void ExamineObject()
        {
            return;
        }

        private void TalkToObject()
        {
            return;
        }

        private void InspectObject()
        {
            return;
        }

        private void DropObject()
        {
            if (heldObject == null)
                return;

            var pickupInteractable = heldObject.GetComponent<PickupInteractable>();
            if (pickupInteractable != null)
            {
                pickupInteractable.Interact(NetworkObject);
                pickupInteractable.enabled = true;
            }

            Vector3 throwDirection = playerCamera.transform.forward;
            Vector3 throwPosition = playerCamera.transform.position + throwDirection * throwDistanceFromCamera;
            DropObjectServerRpc(heldObject.NetworkObjectId, throwDirection, throwPosition);

            heldObject = null;
            heldObjectRigidbody = null;
        }

        [Rpc(SendTo.Server)]
        private void DropObjectServerRpc(ulong objectId, Vector3 throwDirection, Vector3 throwPosition)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
            {
                networkObject.TryRemoveParent();

                networkObject.transform.position = throwPosition;

                PickupInteractable pickup = networkObject.GetComponent<PickupInteractable>();
                if (pickup != null)
                {
                    pickup.SetCollidersEnabled(true);
                }

                Rigidbody rb = networkObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.linearVelocity = throwDirection * throwForce;
                }

                DropObjectClientRpc(objectId, throwDirection, throwPosition);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void DropObjectClientRpc(ulong objectId, Vector3 throwDirection, Vector3 throwPosition)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
            {
                networkObject.transform.position = throwPosition;

                PickupInteractable pickup = networkObject.GetComponent<PickupInteractable>();
                if (pickup != null)
                {
                    pickup.SetCollidersEnabled(true);
                }

                Rigidbody rb = networkObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    if (!IsServer)
                    {
                        rb.linearVelocity = throwDirection * throwForce;
                    }
                }
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Interact();
            }
        }
    }
}