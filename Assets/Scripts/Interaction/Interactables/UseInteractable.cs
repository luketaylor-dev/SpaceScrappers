using Unity.Netcode;
using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(NetworkObject), typeof(OutlineEffect))]
    public class UseInteractable : NetworkBehaviour, IInteractable
    {
        private IInteractable.InteractType interactType = IInteractable.InteractType.Use;
        private OutlineEffect outlineEffect;
        
        private void Awake()
        {
            outlineEffect = GetComponent<OutlineEffect>();
        }
        public IInteractable.InteractType GetInteractType()
        {
            return interactType;
        }

        public void OnHoverStart()
        {
            Debug.Log("Hovering over " + gameObject.name);
            outlineEffect?.EnableOutline();
        }

        public void OnHoverEnd()
        {
            Debug.Log("Not hovering over " + gameObject.name);
            outlineEffect?.DisableOutline();
        }

        public string GetInteractionPrompt()
        {
            return "Use  " + gameObject.name;
        }

        public void Interact(NetworkObject player)
        {
            Debug.Log("You used the " + gameObject.name);
        }
    }
}