using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceScrappers.ShipController
{
    [RequireComponent(typeof(Rigidbody))]
    public class ShipController : MonoBehaviour
    {
        [field: SerializeField] public float MaxSpeed { get; private set; } = 80f;
        [field: SerializeField] public float ThrottleRate { get; private set; } = 0.3f;
        [field: SerializeField] public float PitchSensitivity { get; private set; } = 0.5f;
        [field: SerializeField] public float RollSensitivity { get; private set; } = 0.5f;
        [field: SerializeField] public float AngularDamping { get; private set; } = 5f;
        [field: SerializeField] public float StrafeSpeed { get; private set; } = 15f;
        [field: SerializeField] public InputActionAsset InputActionAsset { get; private set; }

        private float _throttle;
        private Rigidbody _rigidbody;
        // Accumulated between Update and FixedUpdate since pointer delta resets each frame
        private Vector2 _lookAccumulated;

        private InputAction _throttleAction;
        private InputAction _lookAction;
        private InputAction _strafeAction;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.linearDamping = 0f;
            _rigidbody.angularDamping = AngularDamping;

            if (InputActionAsset != null)
            {
                var shipMap = InputActionAsset.FindActionMap("Ship");
                if (shipMap != null)
                {
                    _throttleAction = shipMap.FindAction("Throttle");
                    _lookAction = shipMap.FindAction("Look");
                    _strafeAction = shipMap.FindAction("Strafe");
                }
            }
        }

        private void OnEnable()
        {
            _throttleAction?.Enable();
            _lookAction?.Enable();
            _strafeAction?.Enable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            _throttleAction?.Disable();
            _lookAction?.Disable();
            _strafeAction?.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (_lookAction == null) return;

            // Accumulate delta — pointer delta is per-frame so we collect it here
            // and consume it in FixedUpdate to avoid losing input between physics steps
            _lookAccumulated += _lookAction.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
#if UNITY_EDITOR
            _rigidbody.angularDamping = AngularDamping;
#endif
            // Throttle
            float throttleInput = _throttleAction?.ReadValue<float>() ?? 0f;
            _throttle = Mathf.Clamp01(_throttle + throttleInput * ThrottleRate * Time.fixedDeltaTime);

            // Linear velocity — direct set for space-feel throttle hold
            float strafeInput = _strafeAction?.ReadValue<float>() ?? 0f;
            _rigidbody.linearVelocity = transform.forward * _throttle * MaxSpeed
                                      + transform.right * strafeInput * StrafeSpeed;

            // Rotational torque — builds angular velocity over time (inertia feel)
            // ForceMode.Acceleration ignores mass so inertia tensor shape doesn't matter
            Vector3 torque = transform.right * _lookAccumulated.y * PitchSensitivity
                           - transform.forward * _lookAccumulated.x * RollSensitivity;
            _rigidbody.AddTorque(torque, ForceMode.Acceleration);
            _lookAccumulated = Vector2.zero;
        }

        // Exposed for testing
        public float Throttle => _throttle;

        public void SetThrottleForTest(float value) => _throttle = value;
    }
}
