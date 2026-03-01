using System;
using UnityEngine;

namespace SpaceScrappers.ShipController
{
    /// <summary>
    /// Scene-level manager for ship piloting. Tracks the active ship and handles
    /// the transition between player-on-foot and piloting modes.
    ///
    /// In the sandbox, the first ship found in the scene is auto-assigned.
    /// In the full game, call EnterShip() when the player sits in a cockpit
    /// and ExitShip() when they leave.
    /// </summary>
    public class ShipManager : MonoBehaviour
    {
        public static ShipManager Instance { get; private set; }

        public ShipController ActiveShip { get; private set; }

        /// <summary>Fired whenever the active ship changes. Argument is null when no ship is active.</summary>
        public event Action<ShipController> ActiveShipChanged;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Sandbox auto-assign: find the first ship in the scene so the player
            // can fly immediately without needing to walk into a cockpit.
            // In the full game this would be removed — EnterShip() is called by the
            // cockpit interaction trigger instead.
            var ship = FindFirstObjectByType<ShipController>();
            if (ship != null)
                EnterShip(ship);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        /// <summary>
        /// Call when a player enters a ship's cockpit.
        /// Disables the player FPS controller and activates ship input.
        /// </summary>
        /// <param name="ship">The ship being entered.</param>
        public void EnterShip(ShipController ship)
        {
            if (ship == null) return;

            // TODO: find the local player and disable their FPS controller / input here
            //       e.g. player.GetComponent<NetworkFirstPersonController>().enabled = false;

            ActiveShip = ship;
            ActiveShipChanged?.Invoke(ActiveShip);
        }

        /// <summary>
        /// Call when a player exits a ship's cockpit.
        /// Re-enables the player FPS controller.
        /// </summary>
        public void ExitShip()
        {
            if (ActiveShip == null) return;

            // TODO: re-enable the player FPS controller / input here

            ActiveShip = null;
            ActiveShipChanged?.Invoke(null);
        }
    }
}
