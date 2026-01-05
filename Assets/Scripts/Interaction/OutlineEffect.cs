using UnityEngine;

namespace Interaction
{
    public class OutlineEffect : MonoBehaviour
    {
        [SerializeField] private Material outlineMaterial;
        
        private Renderer[] renderers;
        private Material[][] originalMaterials;
        private bool isOutlineActive;

        private void Awake()
        {
            CacheRenderers();
        }

        private void CacheRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length][];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].materials;
            }
        }

        public void EnableOutline()
        {
            if (isOutlineActive || outlineMaterial == null)
                return;

            if (renderers == null || renderers.Length == 0)
                CacheRenderers();

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    Material[] materials = renderers[i].materials;
                    Material[] newMaterials = new Material[materials.Length + 1];
                    
                    for (int j = 0; j < materials.Length; j++)
                    {
                        newMaterials[j] = materials[j];
                    }
                    
                    newMaterials[materials.Length] = outlineMaterial;
                    
                    renderers[i].materials = newMaterials;
                }
            }

            isOutlineActive = true;
        }

        public void DisableOutline()
        {
            if (!isOutlineActive)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && originalMaterials[i] != null)
                {
                    renderers[i].materials = originalMaterials[i];
                }
            }

            isOutlineActive = false;
        }

        private void OnDestroy()
        {
            if (isOutlineActive)
                DisableOutline();
        }

        public void SetOutlineMaterial(Material material)
        {
            outlineMaterial = material;
        }
    }
}

