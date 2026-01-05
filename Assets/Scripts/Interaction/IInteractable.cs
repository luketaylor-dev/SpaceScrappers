using UnityEngine;

namespace Interaction
{
    public interface IInteractable
    {
        void Interact(GameObject player);
        void OnHoverStart();
        void OnHoverEnd();
        string GetInteractionPrompt();
    }
}