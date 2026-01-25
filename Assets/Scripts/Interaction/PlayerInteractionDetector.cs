using Unity.Netcode;
using UnityEngine;

namespace SpaceScrappers.Interaction
{
    public class PlayerInteractionDetector : NetworkBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactionDistance = 2f;
        [SerializeField] private LayerMask interactionLayer;

        public IInteractable CurrentInteractable { get; private set; }
        public bool HasInteractable => CurrentInteractable != null;

        private void Start()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            CheckForInteractable();
        }

        private void CheckForInteractable()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                
                if (interactable != null)
                {
                    SetCurrentInteractable(interactable);
                }
                else
                {
                    ClearCurrentInteractable();
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }

        private void SetCurrentInteractable(IInteractable interactable)
        {
            if (CurrentInteractable == null)
            {
                CurrentInteractable = interactable;
                CurrentInteractable.OnHoverStart();
            }
            else if (CurrentInteractable != interactable)
            {
                CurrentInteractable.OnHoverEnd();
                CurrentInteractable = interactable;
                CurrentInteractable.OnHoverStart();
            }
        }

        private void ClearCurrentInteractable()
        {
            if (CurrentInteractable != null)
            {
                if (CurrentInteractable is Object obj && obj != null)
                {
                    CurrentInteractable.OnHoverEnd();
                }

                CurrentInteractable = null;
            }
        }

        public Vector3 GetCameraForward()
        {
            return playerCamera != null ? playerCamera.transform.forward : transform.forward;
        }

        public Vector3 GetCameraPosition()
        {
            return playerCamera != null ? playerCamera.transform.position : transform.position;
        }
    }
}
