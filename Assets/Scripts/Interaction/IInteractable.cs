using Unity.Netcode;
using UnityEngine;

namespace Interaction
{
    public interface IInteractable
    {
        enum InteractType
        {
            Pickup,
            Use,
            Examine,
            Talk,
            Inspect
        }

        InteractType GetInteractType();
        void OnHoverStart();
        void OnHoverEnd();
        string GetInteractionPrompt();
        void Interact(NetworkObject player);
    }
}