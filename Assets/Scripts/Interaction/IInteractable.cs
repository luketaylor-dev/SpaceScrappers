using Unity.Netcode;
using UnityEngine;

namespace SpaceScrappers.Interaction
{
    public interface IInteractable
    {
        enum InteractType
        {
            Pickup,
            Use
        }

        InteractType GetInteractType();
        void OnHoverStart();
        void OnHoverEnd();
        string GetInteractionPrompt();
        void Interact(NetworkObject player);
    }
}