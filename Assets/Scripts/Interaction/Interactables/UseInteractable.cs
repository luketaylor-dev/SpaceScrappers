using SpaceScrappers.Debugging;
using Unity.Netcode;
using UnityEngine;

namespace SpaceScrappers.Interaction
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