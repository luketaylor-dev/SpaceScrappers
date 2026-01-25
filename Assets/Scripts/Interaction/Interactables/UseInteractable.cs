using SpaceScrappers.Debugging;
using Unity.Netcode;
using UnityEngine;

namespace SpaceScrappers.Interaction
{
    [RequireComponent(typeof(NetworkObject), typeof(StencilOutlineEffect))]
    public class UseInteractable : NetworkBehaviour, IInteractable
    {
        private IInteractable.InteractType interactType = IInteractable.InteractType.Use;
        private StencilOutlineEffect outlineEffect;

        private void Awake()
        {
            outlineEffect = GetComponent<StencilOutlineEffect>();
        }
        public IInteractable.InteractType GetInteractType()
        {
            return interactType;
        }

        public void OnHoverStart()
        {
            outlineEffect?.EnableOutline();
        }

        public void OnHoverEnd()
        {
            outlineEffect?.DisableOutline();
        }

        public string GetInteractionPrompt()
        {
            return "Use  " + gameObject.name;
        }

        public void Interact(NetworkObject player)
        {
            GameLogger.Log($"Used {gameObject.name}");
        }
    }
}