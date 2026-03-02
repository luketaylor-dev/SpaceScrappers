using UnityEngine;

namespace SpaceScrappers.Player
{
    /// <summary>
    /// Plain C# spring-constraint tether. No MonoBehaviour.
    /// Supports both static world anchors and dynamic Rigidbody anchors.
    /// The spring is unilateral — it only pulls when taut (distance > Length).
    /// </summary>
    public class Tether
    {
        public float Length { get; private set; }
        public float Stiffness { get; }
        public float MaxTension { get; }

        private readonly Rigidbody _anchorBody;
        private readonly Vector3 _localOffset;
        private readonly Vector3 _staticAnchorWorld;

        /// <summary>Static anchor at a fixed world position.</summary>
        public Tether(Vector3 staticAnchorWorld, float length, float stiffness = 150f, float maxTension = 500f)
        {
            _staticAnchorWorld = staticAnchorWorld;
            Length = length;
            Stiffness = stiffness;
            MaxTension = maxTension;
        }

        /// <summary>Dynamic anchor on a Rigidbody (localOffset in body-local space).</summary>
        public Tether(Rigidbody anchorBody, Vector3 localOffset, float length, float stiffness = 150f,
            float maxTension = 500f)
        {
            _anchorBody = anchorBody;
            _localOffset = localOffset;
            Length = length;
            Stiffness = stiffness;
            MaxTension = maxTension;
        }

        /// <summary>Current anchor position in world space.</summary>
        public Vector3 GetAnchorWorldPosition()
        {
            return _anchorBody != null
                ? _anchorBody.transform.TransformPoint(_localOffset)
                : _staticAnchorWorld;
        }

        /// <summary>
        /// Applies spring constraint force to the player Rigidbody.
        /// Returns true if the tether broke (force exceeded MaxTension) and should be discarded.
        /// </summary>
        public bool ApplyConstraintForce(Rigidbody player)
        {
            var anchorPos = GetAnchorWorldPosition();
            var toAnchor = anchorPos - player.position;
            var distance = toAnchor.magnitude;

            if (distance <= Length)
                return false; // slack — no force applied

            float stretch = distance - Length;
            var force = toAnchor.normalized * (stretch * Stiffness);

            if (MaxTension > 0 && force.magnitude > MaxTension)
            {
                Debug.Log("Tether broke");
                return true;
            }

            player.AddForce(force, ForceMode.Force);

            // Newton's 3rd — push the dynamic anchor equally in the opposite direction
            if (_anchorBody != null)
                _anchorBody.AddForce(-force, ForceMode.Force);

            return false;
        }

        /// <summary>Reel the tether in or out, clamped to [min, max].</summary>
        public void AdjustLength(float delta, float min = 0.5f, float max = 50f)
        {
            Length = Mathf.Clamp(Length + delta, min, max);
        }
    }
}