using UnityEngine;
using UnityEngine.InputSystem;

namespace Interaction
{
    public class PlayerInteraction : MonoBehaviour
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

        private void OnEnable()
        {
            Debug.Log("Player interaction enabled " + interactAction.name);
            if (interactAction != null)
            {
                Debug.Log("Interact action set to: " + interactAction.name);
                interactAction.Enable();
                interactAction.performed += OnInteract;
            }
        }

        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.performed -= OnInteract;
                interactAction.Disable();
            }
        }

        private void Update()
        {
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
            if (currentInteractable != null)
            {
               currentInteractable.Interact(gameObject);
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