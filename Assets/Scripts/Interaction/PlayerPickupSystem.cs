using Unity.Netcode;
using UnityEngine;

namespace SpaceScrappers.Interaction
{
    public class PlayerPickupSystem : NetworkBehaviour
    {
        [Header("Pickup Settings")]
        [SerializeField] private Transform holdSpotPosition;
        [SerializeField] private float throwForce = 10f;
        [SerializeField] private float throwDistanceFromCamera = 1.5f;

        public NetworkObject HeldObject { get; private set; }
        public bool IsHoldingObject => HeldObject != null;

        private PlayerInteractionDetector detector;

        private void Awake()
        {
            detector = GetComponent<PlayerInteractionDetector>();
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            UpdateHeldObjectPosition();
        }

        private void UpdateHeldObjectPosition()
        {
            if (HeldObject != null && holdSpotPosition != null)
            {
                HeldObject.transform.position = holdSpotPosition.position;
                HeldObject.transform.rotation = holdSpotPosition.rotation;
            }
        }

        public bool TryPickup(PickupInteractable pickupInteractable)
        {
            if (!IsOwner || IsHoldingObject)
                return false;

            NetworkObject networkObject = pickupInteractable.NetworkObject;
            
            if (networkObject == null)
            {
                Debug.LogError("Item is not a NetworkObject");
                return false;
            }

            if (!networkObject.IsSpawned)
            {
                Debug.LogError("NetworkObject is not spawned, cannot pickup");
                return false;
            }

            pickupInteractable.Interact(NetworkObject);
            HeldObject = networkObject;

            PickupObjectServerRpc(networkObject.NetworkObjectId);
            pickupInteractable.enabled = false;

            return true;
        }

        public void Drop()
        {
            if (!IsOwner || !IsHoldingObject)
                return;

            PickupInteractable pickupInteractable = HeldObject.GetComponent<PickupInteractable>();
            if (pickupInteractable != null)
            {
                pickupInteractable.Interact(NetworkObject);
                pickupInteractable.enabled = true;
            }

            Vector3 throwDirection = detector.GetCameraForward();
            Vector3 throwPosition = detector.GetCameraPosition() + throwDirection * throwDistanceFromCamera;
            
            DropObjectServerRpc(HeldObject.NetworkObjectId, throwDirection, throwPosition);

            HeldObject = null;
        }

        [Rpc(SendTo.Server)]
        private void PickupObjectServerRpc(ulong objectId)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
                return;

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

        [Rpc(SendTo.ClientsAndHost)]
        private void PickupObjectClientRpc(ulong objectId)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
                return;

            if (IsServer)
                return;

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

        [Rpc(SendTo.Server)]
        private void DropObjectServerRpc(ulong objectId, Vector3 throwDirection, Vector3 throwPosition)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
                return;

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

        [Rpc(SendTo.ClientsAndHost)]
        private void DropObjectClientRpc(ulong objectId, Vector3 throwDirection, Vector3 throwPosition)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
                return;

            if (IsServer)
                return;

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
        }
    }
}
