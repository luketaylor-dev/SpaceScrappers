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
            if (!IsOwner || currentInteractable == null)
                return;

            currentInteractable.Interact(gameObject);
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