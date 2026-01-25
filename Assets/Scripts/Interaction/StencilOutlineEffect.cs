using UnityEngine;

namespace SpaceScrappers.Interaction
{
    public class StencilOutlineEffect : MonoBehaviour
    {
        [SerializeField] private Color outlineColor = Color.yellow;
        [SerializeField] private float outlineWidth = 0.03f;
        [SerializeField] private Material outlineMaterial;

        private Renderer[] renderers;
        private Material[][] originalMaterials;
        private Material[][] materialsWithOutline;
        private Material outlineInstance;
        private bool isOutlineActive;
        private bool isInitialized;

        private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthProperty = Shader.PropertyToID("_OutlineWidth");

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length][];
            materialsWithOutline = new Material[renderers.Length][];

            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].sharedMaterials;
            }

            if (outlineMaterial == null)
            {
                Debug.LogError($"Outline material not assigned on {gameObject.name}");
            }
        }

        private void InitializeOutlineMaterials()
        {
            if (isInitialized || outlineMaterial == null) return;

            outlineInstance = new Material(outlineMaterial);
            outlineInstance.SetColor(OutlineColorProperty, outlineColor);
            outlineInstance.SetFloat(OutlineWidthProperty, outlineWidth);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (originalMaterials[i] != null)
                {
                    materialsWithOutline[i] = new Material[originalMaterials[i].Length + 1];

                    for (int j = 0; j < originalMaterials[i].Length; j++)
                    {
                        materialsWithOutline[i][j] = originalMaterials[i][j];
                    }

                    materialsWithOutline[i][originalMaterials[i].Length] = outlineInstance;
                }
            }

            isInitialized = true;
        }

        public void EnableOutline()
        {
            if (isOutlineActive || outlineMaterial == null) return;

            if (!isInitialized)
            {
                InitializeOutlineMaterials();
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && materialsWithOutline[i] != null)
                {
                    renderers[i].materials = materialsWithOutline[i];
                }
            }

            isOutlineActive = true;
        }

        public void DisableOutline()
        {
            if (!isOutlineActive) return;

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

            if (outlineInstance != null)
            {
                Destroy(outlineInstance);
            }
        }

        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            if (outlineInstance != null)
            {
                outlineInstance.SetColor(OutlineColorProperty, color);
            }
        }

        public void SetOutlineWidth(float width)
        {
            outlineWidth = width;
            if (outlineInstance != null)
            {
                outlineInstance.SetFloat(OutlineWidthProperty, width);
            }
        }
    }
}
