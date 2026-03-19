using UnityEngine;

namespace SpaceScrappers.Player
{
    /// <summary>
    /// Manages up to two active Tether instances.
    /// Tether[0] is the primary movement anchor; Tether[1] is the secondary anchor.
    /// Reeling Tether[0] in counter-adjusts Tether[1] by the same delta, and vice versa.
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

        public Tether[] ActiveTether { get; private set; }

        private static readonly RaycastHit[] s_hitBuffer = new RaycastHit[1];

        private PlayerInputController _input;
        private Rigidbody _rb;

        private void Awake()
        {
            _input = GetComponent<PlayerInputController>();
            _rb = GetComponent<Rigidbody>();
            ActiveTether = new Tether[2];
        }

        private void Update()
        {
            if (_input.IsTetherFirePressed)
                TryFireTether(0);

            if (_input.IsAltTetherFirePressed)
                TryFireTether(1);

            if (_input.IsTetherReleasePressed)
                ReleaseTether();
        }

        private void FixedUpdate()
        {
            // Consume once — reel[0] in/out counter-adjusts tether[1] by the same delta
            float reelDelta = -_input.ConsumeReelAxis() * reelSpeed;

            if (ActiveTether[0] != null)
            {
                ActiveTether[0].AdjustLength(reelDelta, minLength, maxLength);

                if (ActiveTether[0].ApplyConstraintForce(_rb))
                    ActiveTether[0] = null;
            }
            if (ActiveTether[1] != null)
            {
                ActiveTether[1].AdjustLength(-reelDelta, minLength, maxLength);

                if (ActiveTether[1].ApplyConstraintForce(_rb))
                    ActiveTether[1] = null;
            }
        }

        private void TryFireTether(int slot)
        {
            int hitCount = Physics.RaycastNonAlloc(
                cameraTransform.position,
                cameraTransform.forward,
                s_hitBuffer,
                maxRange);

            if (hitCount == 0) return;

            var hit = s_hitBuffer[0];

            ActiveTether[slot] = hit.rigidbody != null
                ? new Tether(hit.rigidbody, hit.rigidbody.transform.InverseTransformPoint(hit.point), hit.distance, stiffness, maxTension)
                : new Tether(hit.point, hit.distance, stiffness, maxTension);
        }

        private void ReleaseTether()
        {
            ActiveTether[0] = null;
            ActiveTether[1] = null;
        }
    }
}