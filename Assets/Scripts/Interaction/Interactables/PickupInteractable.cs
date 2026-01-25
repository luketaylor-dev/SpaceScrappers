using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SpaceScrappers.Interaction
{
    [RequireComponent(typeof(NetworkObject), typeof(Rigidbody), typeof(OutlineEffect))]
    public class PickupInteractable : NetworkBehaviour, IInteractable
    {
        private OutlineEffect outlineEffect;

        private readonly IInteractable.InteractType interactType = IInteractable.InteractType.Pickup;

        private NetworkObject currentHolder;
        
        private Collider objectCollider;
        private Collider lastThrowerCollider;
        private Collider[] cachedColliders;
        private float ignoreCollisionTime = 0.5f;

        private void Awake()
        {
            outlineEffect = GetComponent<OutlineEffect>();
            objectCollider = GetComponent<Collider>();
            cachedColliders = GetComponentsInChildren<Collider>();
        }

        public void OnHoverStart()
        {
            if (currentHolder) return;
            Debug.Log("Hovering over " + gameObject.name);
            outlineEffect?.EnableOutline();
        }

        public void OnHoverEnd()
        {
            if (currentHolder) return;
            Debug.Log("Not hovering over " + gameObject.name);
            outlineEffect?.DisableOutline();
        }

        public string GetInteractionPrompt()
        {
            return "Interact with " + gameObject.name;
        }

        public IInteractable.InteractType GetInteractType()
        {
            return interactType;
        }

        public void Interact(NetworkObject player)
        {
            if (currentHolder != null)
            {
                Collider playerCollider = player.GetComponent<Collider>();
                if (playerCollider != null && objectCollider != null)
                {
                    StartCoroutine(IgnoreCollisionTemporarily(playerCollider));
                }
                ChangeOwnershipServerRpc(NetworkManager.ServerClientId, 0, true);
                return;
            }
            OnHoverEnd();
            ChangeOwnershipServerRpc(player.OwnerClientId, player.NetworkObjectId, false);
        }

        private IEnumerator IgnoreCollisionTemporarily(Collider playerCollider)
        {
            Physics.IgnoreCollision(objectCollider, playerCollider, true);
            yield return new WaitForSeconds(ignoreCollisionTime);
            Physics.IgnoreCollision(objectCollider, playerCollider, false);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ChangeOwnershipServerRpc(ulong newOwnerId, ulong holderObjectId, bool isDropping)
        {
            if (isDropping)
            {
                currentHolder = null;
            }
            else
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(holderObjectId, out NetworkObject holderObject))
                {
                    currentHolder = holderObject;
                }
            }

            NetworkObject.ChangeOwnership(newOwnerId);
        }

        public void SetCollidersEnabled(bool enabled)
        {
            if (cachedColliders == null)
                return;

            foreach (Collider col in cachedColliders)
            {
                if (col != null)
                    col.enabled = enabled;
            }
        }
    }
}