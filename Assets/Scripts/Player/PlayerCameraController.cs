using UnityEngine;

namespace SpaceScrappers.Player
{
    /// <summary>
    /// Rotates the player's Rigidbody to match mouse look — pitch and yaw both affect the full body,
    /// so the player physically spins in zero-G. The camera child follows as a simple position offset.
    /// </summary>
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Sensitivity")]
        [SerializeField] private float pitchSensitivity = 0.15f;
        [SerializeField] private float yawSensitivity   = 0.15f;

        private PlayerInputController _input;
        private Rigidbody             _rb;

        private void Awake()
        {
            _input = GetComponent<PlayerInputController>();
            _rb    = GetComponent<Rigidbody>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void FixedUpdate()
        {
            var delta = _input.ConsumeLookDelta();

            // Both axes use body-local axes so turning always matches what's on screen
            // regardless of orientation — world-up yaw inverts when the player is upside-down
            var yaw   = Quaternion.AngleAxis( delta.x * yawSensitivity,   _rb.rotation * Vector3.up);
            var pitch = Quaternion.AngleAxis(-delta.y * pitchSensitivity, _rb.rotation * Vector3.right);

            _rb.MoveRotation(yaw * pitch * _rb.rotation);
        }
    }
}
