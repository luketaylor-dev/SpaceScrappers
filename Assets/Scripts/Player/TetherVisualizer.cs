using UnityEngine;

namespace SpaceScrappers.Player
{
    /// <summary>
    /// Renders the active tether using a LineRenderer.
    /// Place on a child GameObject of the player with a LineRenderer component.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TetherVisualizer : MonoBehaviour
    {
        [SerializeField] private TetherController tetherController;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            bool hasTether = tetherController.ActiveTether != null;
            _lineRenderer.enabled = hasTether;

            if (!hasTether) return;

            _lineRenderer.SetPosition(0, tetherController.transform.position);
            _lineRenderer.SetPosition(1, tetherController.ActiveTether.GetAnchorWorldPosition());
        }
    }
}