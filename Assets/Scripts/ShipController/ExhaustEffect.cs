using UnityEngine;

namespace SpaceScrappers.ShipController
{
    /// <summary>
    /// Updates child particle system velocities every frame so exhaust always
    /// travels backward relative to the active ship, regardless of orientation.
    /// </summary>
    public class ExhaustEffect : MonoBehaviour
    {
        [field: SerializeField] public float ExhaustSpeed { get; private set; } = 12f;

        private ParticleSystem[] _systems;

        private void Awake()
        {
            _systems = GetComponentsInChildren<ParticleSystem>();
        }

        private void LateUpdate()
        {
            var ship = ShipManager.Instance?.ActiveShip;
            if (ship == null) return;

            var backward = -ship.transform.forward * ExhaustSpeed;

            foreach (var ps in _systems)
            {
                var vel = ps.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.World;
                vel.x = backward.x;
                vel.y = backward.y;
                vel.z = backward.z;
            }
        }
    }
}
