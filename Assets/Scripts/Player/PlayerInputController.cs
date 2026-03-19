using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceScrappers.Player
{
    /// <summary>
    /// Input hub. Owns and enables the "Player" action map.
    /// All sibling components read from this — never poll InputSystem directly.
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        [field: SerializeField] public InputActionAsset InputActionAsset { get; private set; }

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _thrusterUpDownAction;
        private InputAction _tetherFireAction;
        private InputAction _altTetherFireAction;
        private InputAction _tetherReleaseAction;
        private InputAction _reelAxisAction;

        private Vector2 _lookAccumulated;
        private float _reelAccumulated;

        public Vector2 MoveInput { get; private set; }
        public float ThrusterUpDownInput { get; private set; }
        public bool IsTetherFirePressed { get; private set; }
        public bool IsAltTetherFirePressed { get; private set; }
        public bool IsTetherReleasePressed { get; private set; }

        private void Awake()
        {
            var map = InputActionAsset.FindActionMap("Player", throwIfNotFound: true);

            _moveAction = map.FindAction("Move", throwIfNotFound: true);
            _lookAction = map.FindAction("Look", throwIfNotFound: true);
            _thrusterUpDownAction = map.FindAction("ThrusterUpDown", throwIfNotFound: true);
            _tetherFireAction = map.FindAction("TetherFire", throwIfNotFound: true);
            _altTetherFireAction = map.FindAction("AltTetherFire", throwIfNotFound: true);
            _tetherReleaseAction = map.FindAction("TetherRelease", throwIfNotFound: true);
            _reelAxisAction = map.FindAction("ReelAxis", throwIfNotFound: true);

            map.Enable();
        }

        private void OnDestroy()
        {
            InputActionAsset.FindActionMap("Player")?.Disable();
        }

        private void Update()
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            ThrusterUpDownInput = _thrusterUpDownAction.ReadValue<float>();

            // Scroll and look are per-frame impulses — accumulate so FixedUpdate gets the full delta
            _reelAccumulated += _reelAxisAction.ReadValue<float>();
            _lookAccumulated += _lookAction.ReadValue<Vector2>();

            // WasPressedThisFrame is stable for the whole frame regardless of component Update order
            IsTetherFirePressed = _tetherFireAction.WasPressedThisFrame();
            IsAltTetherFirePressed = _altTetherFireAction.WasPressedThisFrame();
            IsTetherReleasePressed = _tetherReleaseAction.WasPressedThisFrame();
        }

        /// <summary>Returns accumulated scroll delta since last call and resets the accumulator.</summary>
        public float ConsumeReelAxis()
        {
            var delta = _reelAccumulated;
            _reelAccumulated = 0f;
            return delta;
        }

        /// <summary>Returns accumulated look delta since last call and resets the accumulator.</summary>
        public Vector2 ConsumeLookDelta()
        {
            var delta = _lookAccumulated;
            _lookAccumulated = Vector2.zero;
            return delta;
        }
    }
}