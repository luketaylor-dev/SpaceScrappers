using SpaceScrappers.Debugging;
using Unity.Netcode;
using UnityEngine;

namespace SpaceScrappers.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerKnockback : NetworkBehaviour
    {
        [SerializeField] private FirstPersonController fpsController;
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private float knockbackDuration = 2f;
        [SerializeField] private float minVelocityForKnockback = 5f;
        [SerializeField] private float minUpwardKnockback = 0.3f;
        [SerializeField] private float velocityForceScale = 10f;
        [SerializeField] private Vector2 torqueRangeX = new Vector2(-1f, 1f);
        [SerializeField] private Vector2 torqueRangeY = new Vector2(-0.5f, 0.5f);
        [SerializeField] private Vector2 torqueRangeZ = new Vector2(-1f, 1f);
        [SerializeField] private float torqueMultiplier = 0.5f;

        private const float BASE_FORCE_MULTIPLIER = 1f;

        private Rigidbody playerRigidbody;
        private RigidbodyConstraints originalConstraints;
        private bool isKnockedDown;

        private void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            if (fpsController == null)
                fpsController = GetComponent<FirstPersonController>();

            if (playerRigidbody != null)
                originalConstraints = playerRigidbody.constraints;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsOwner || isKnockedDown)
                return;

            if (!collision.gameObject.CompareTag("Projectile"))
                return;

            Rigidbody otherRigidbody = collision.rigidbody;
            if (otherRigidbody == null)
                return;

            float collisionVelocity = otherRigidbody.linearVelocity.magnitude;
            if (collisionVelocity < minVelocityForKnockback)
                return;

            Vector3 knockbackDirection = collision.relativeVelocity.normalized;
            knockbackDirection.y = Mathf.Max(minUpwardKnockback, knockbackDirection.y);

            float forceMultiplier = Mathf.Clamp01((collisionVelocity - minVelocityForKnockback) / velocityForceScale);
            float finalForceMultiplier = BASE_FORCE_MULTIPLIER + forceMultiplier;

            NetworkObject projectileNetworkObject = collision.gameObject.GetComponent<NetworkObject>();

            if (projectileNetworkObject != null && projectileNetworkObject.IsSpawned)
            {
                ApplyKnockbackServerRpc(projectileNetworkObject.NetworkObjectId, knockbackDirection,
                    finalForceMultiplier);
            }
            else
            {
                StartCoroutine(KnockdownCoroutine(knockbackDirection, finalForceMultiplier));
            }
        }

        [Rpc(SendTo.Server)]
        private void ApplyKnockbackServerRpc(ulong projectileObjectId, Vector3 direction, float forceMultiplier)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(projectileObjectId,
                    out NetworkObject projectileNetworkObject))
            {

            }

            ApplyKnockbackClientRpc(direction, forceMultiplier);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ApplyKnockbackClientRpc(Vector3 direction, float forceMultiplier)
        {
            if (IsOwner && !isKnockedDown)
            {
                StartCoroutine(KnockdownCoroutine(direction, forceMultiplier));
            }
        }

        private System.Collections.IEnumerator KnockdownCoroutine(Vector3 direction, float forceMultiplier)
        {
            isKnockedDown = true;

            if (fpsController != null)
                fpsController.enabled = false;

            if (playerRigidbody != null)
            {
                originalConstraints = playerRigidbody.constraints;
                playerRigidbody.constraints = RigidbodyConstraints.None;

                Vector3 knockbackForceVector = direction * (knockbackForce * forceMultiplier);
                playerRigidbody.AddForce(knockbackForceVector, ForceMode.Impulse);

                Vector3 torque = new Vector3(
                    Random.Range(torqueRangeX.x, torqueRangeX.y),
                    Random.Range(torqueRangeY.x, torqueRangeY.y),
                    Random.Range(torqueRangeZ.x, torqueRangeZ.y)
                ) * (knockbackForce * torqueMultiplier);
                playerRigidbody.AddTorque(torque, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(knockbackDuration);

            if (playerRigidbody != null)
            {
                Vector3 eulerAngles = transform.rotation.eulerAngles;
                Quaternion uprightRotation = Quaternion.Euler(0f, eulerAngles.y, 0f);

                transform.rotation = uprightRotation;

                playerRigidbody.constraints = originalConstraints;
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }

            if (fpsController != null)
                fpsController.enabled = true;

            isKnockedDown = false;
        }
    }
}