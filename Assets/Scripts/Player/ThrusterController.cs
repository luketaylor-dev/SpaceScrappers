using UnityEngine;

namespace SpaceScrappers.Player
{
    /// <summary>
    /// Applies thruster forces from WASD and Space/Ctrl inputs in body-local space.
    /// Because the body physically rotates with the camera, body-local always matches
    /// what the player sees on screen regardless of orientation.
    /// </summary>
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(Rigidbody))]
    public class ThrusterController : MonoBehaviour
    {
        [Header("Force")]
        [SerializeField] private float thrustForce         = 20f;
        [SerializeField] private float verticalThrustForce = 15f;

        private PlayerInputController _input;
        private Rigidbody             _rb;

        private void Awake()
        {
            _input = GetComponent<PlayerInputController>();
            _rb    = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            var move   = _input.MoveInput;
            var upDown = _input.ThrusterUpDownInput;

            var lateralForce  = (transform.right * move.x + transform.forward * move.y) * thrustForce;
            var verticalForce = transform.up * (upDown * verticalThrustForce);

            _rb.AddForce(lateralForce + verticalForce, ForceMode.Force);
        }
    }
}
