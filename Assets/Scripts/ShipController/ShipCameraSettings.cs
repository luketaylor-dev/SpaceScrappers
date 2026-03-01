using UnityEngine;

namespace SpaceScrappers.ShipController
{
    /// <summary>
    /// Per-ship camera configuration. Lives on the ship so different ship types
    /// (fighter, battleship, etc.) can define their own camera behaviour.
    /// Read at runtime by ShipCameraController via ShipManager.ActiveShip.
    /// </summary>
    public class ShipCameraSettings : MonoBehaviour
    {
        [field: SerializeField] public float FollowDistance { get; private set; } = 25f;
        [field: SerializeField] public float FollowHeight { get; private set; } = 3f;
        [field: SerializeField] public float FollowSmoothing { get; private set; } = 5f;
        [field: SerializeField] public Vector3 CockpitOffset { get; private set; } = new Vector3(0f, 0.5f, 1.5f);
    }
}
