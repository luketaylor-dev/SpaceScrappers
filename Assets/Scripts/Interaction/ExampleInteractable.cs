using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(OutlineEffect))]
    public class ExampleInteractable : MonoBehaviour, IInteractable
    {
        private OutlineEffect outlineEffect;

        private void Awake()
        {
            outlineEffect = GetComponent<OutlineEffect>();
        }

        public void Interact(GameObject player)
        {
            Debug.Log("Interacted with " + gameObject.name);
            Destroy(gameObject);
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
            return "Interact with " + gameObject.name;
        }
    }
}