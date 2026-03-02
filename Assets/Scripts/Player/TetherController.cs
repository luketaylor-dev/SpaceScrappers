using UnityEngine;

namespace SpaceScrappers.Player
{
    /// <summary>
    /// Manages the active Tether lifecycle: fire (raycast), release, reel.
    /// Fire/release are sampled in Update; constraint force is applied in FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(Rigidbody))]
    public class TetherController : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Transform cameraTransform;

        [Header("Tether Settings")] [SerializeField]
        private float maxRange = 50f;

        [SerializeField] private float stiffness = 150f;
        [SerializeField] private float maxTension = 500f;
        [SerializeField] private float reelSpeed = 5f;
        [SerializeField] private float minLength = 0.5f;
        [SerializeField] private float maxLength = 50f;

        public Tether ActiveTether { get; private set; }

        // RaycastNonAlloc avoids per-frame GC allocation
        private static readonly RaycastHit[] s_hitBuffer = new RaycastHit[1];

        private PlayerInputController _input;
        private Rigidbody _rb;

        private void Awake()
        {
            _input = GetComponent<PlayerInputController>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (_input.IsTetherFirePressed)
                TryFireTether();

            if (_input.IsTetherReleasePressed)
                ReleaseTether();
        }

        private void FixedUpdate()
        {
            if (ActiveTether == null) return;

            // Scroll is a per-frame impulse, not a held axis — no Time.fixedDeltaTime
            float reelDelta = -_input.ConsumeReelAxis() * reelSpeed;
            ActiveTether.AdjustLength(reelDelta, minLength, maxLength);

            if (ActiveTether.ApplyConstraintForce(_rb))
                ReleaseTether();
        }

        private void TryFireTether()
        {
            int hitCount = Physics.RaycastNonAlloc(
                cameraTransform.position,
                cameraTransform.forward,
                s_hitBuffer,
                maxRange);

            if (hitCount == 0) return;

            var hit = s_hitBuffer[0];
            float dist = hit.distance;

            if (hit.rigidbody != null)
            {
                var localOffset = hit.rigidbody.transform.InverseTransformPoint(hit.point);
                ActiveTether = new Tether(hit.rigidbody, localOffset, dist, stiffness, maxTension);
            }
            else
            {
                ActiveTether = new Tether(hit.point, dist, stiffness, maxTension);
            }
        }

        private void ReleaseTether()
        {
            ActiveTether = null;
        }
    }
}