using UnityEngine;

namespace SpaceScrappers.Player
{
    /// <summary>
    /// Renders one tether using a LineRenderer.
    /// Set tetherIndex to 0 for the primary tether, 1 for the secondary.
    /// Place on a child GameObject of the player with a LineRenderer component.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TetherVisualizer : MonoBehaviour
    {
        [SerializeField] private TetherController tetherController;
        [SerializeField] private int tetherIndex = 0;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            var tether = tetherController.ActiveTether[tetherIndex];
            if (tether == null)
            {
                _lineRenderer.enabled = false;
                return;
            }

            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, tetherController.transform.position);
            _lineRenderer.SetPosition(1, tether.GetAnchorWorldPosition());
        }
    }
}