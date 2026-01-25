using Football;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Interaction
{
    public class PlayerInteraction : NetworkBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactionDistance = 2f;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private InputActionReference interactActionReference;

        [SerializeField] private Transform holdSpotPosition;
        [SerializeField] private float throwForce = 10f;

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
            Debug.Log("Player camera set to: " + playerCamera);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                if (interactAction != null)
                {
                    Debug.Log("Player interaction enabled " + interactAction.name);
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

        [ServerRpc]
        private void PickupObjectServerRpc(ulong objectId)
        {
            Debug.Log("Pickup");
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
            {
                Debug.Log("Pickup has value");

                networkObject.ChangeOwnership(OwnerClientId);

                Rigidbody rb = networkObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                Collider[] colliders = networkObject.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = false;
                }

                if (networkObject.TrySetParent(NetworkObject))
                {
                    Debug.Log("Server: Pickup parent set to player NetworkObject");
                }

                PickupObjectClientRpc(objectId);
            }
        }

        [ClientRpc]
        private void PickupObjectClientRpc(ulong objectId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
            {
                Rigidbody rb = networkObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                Collider[] colliders = networkObject.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = false;
                }

                if (networkObject.TrySetParent(NetworkObject))
                {
                    Debug.Log("Pickup parent set to player NetworkObject");
                }
                else
                {
                    Debug.LogError("Failed to set parent to player NetworkObject");
                }
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
            Vector3 throwPosition = playerCamera.transform.position + throwDirection * 1.5f;
            DropObjectServerRpc(heldObject.NetworkObjectId, throwDirection, throwPosition);

            heldObject = null;
            heldObjectRigidbody = null;
        }

        [ServerRpc]
        private void DropObjectServerRpc(ulong objectId, Vector3 throwDirection, Vector3 throwPosition)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
            {
                NetworkObject parentNetworkObject = null;
                if (ProjectileDropper.Instance != null)
                {
                    parentNetworkObject = ProjectileDropper.Instance.GetComponent<NetworkObject>();
                }

                if (networkObject.TrySetParent(parentNetworkObject))
                {
                    Debug.Log("Server: Object unparented/reparented successfully");
                }

                networkObject.transform.position = throwPosition;

                Collider[] colliders = networkObject.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = true;
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

        [ClientRpc]
        private void DropObjectClientRpc(ulong objectId, Vector3 throwDirection, Vector3 throwPosition)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
            {
                networkObject.transform.position = throwPosition;

                Collider[] colliders = networkObject.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = true;
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

                Debug.Log("Object dropped and thrown");
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            Debug.Log("Interact button pressed");
            if (context.performed)
            {
                Debug.Log("Interact button performed");
                Interact();
            }
        }
    }
}