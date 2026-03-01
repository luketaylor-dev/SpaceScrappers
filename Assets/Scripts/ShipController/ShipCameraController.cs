using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceScrappers.ShipController
{
    /// <summary>
    /// Follows the active ship using per-ship camera settings defined on
    /// ShipCameraSettings. Subscribes to ShipManager.ActiveShipChanged so the
    /// camera automatically re-configures when the player switches ships.
    /// </summary>
    public class ShipCameraController : MonoBehaviour
    {
        [field: SerializeField] public InputActionAsset InputActionAsset { get; private set; }

        private enum CameraMode { ThirdPerson, Cockpit }
        private CameraMode _currentMode = CameraMode.ThirdPerson;
        private InputAction _toggleAction;

        private Transform _shipTransform;
        private ShipCameraSettings _settings;

        private void Awake()
        {
            if (InputActionAsset != null)
            {
                var shipMap = InputActionAsset.FindActionMap("Ship");
                _toggleAction = shipMap?.FindAction("CameraToggle");
            }
        }

        private void Start()
        {
            if (ShipManager.Instance == null) return;

            ShipManager.Instance.ActiveShipChanged += OnActiveShipChanged;
            OnActiveShipChanged(ShipManager.Instance.ActiveShip);
        }

        private void OnDestroy()
        {
            if (ShipManager.Instance != null)
                ShipManager.Instance.ActiveShipChanged -= OnActiveShipChanged;
        }

        private void OnEnable()
        {
            _toggleAction?.Enable();
            if (_toggleAction != null)
                _toggleAction.performed += OnToggle;
        }

        private void OnDisable()
        {
            if (_toggleAction != null)
                _toggleAction.performed -= OnToggle;
            _toggleAction?.Disable();
        }

        private void OnActiveShipChanged(ShipController ship)
        {
            _shipTransform = ship != null ? ship.transform : null;
            _settings = ship != null ? ship.GetComponent<ShipCameraSettings>() : null;
            _currentMode = CameraMode.ThirdPerson;
        }

        private void OnToggle(InputAction.CallbackContext ctx)
        {
            _currentMode = _currentMode == CameraMode.ThirdPerson
                ? CameraMode.Cockpit
                : CameraMode.ThirdPerson;
        }

        private void LateUpdate()
        {
            if (_shipTransform == null || _settings == null) return;

            if (_currentMode == CameraMode.ThirdPerson)
            {
                Vector3 targetPos = _shipTransform.position
                    - _shipTransform.forward * _settings.FollowDistance
                    + _shipTransform.up * _settings.FollowHeight;
                transform.position = Vector3.Lerp(transform.position, targetPos, _settings.FollowSmoothing * Time.deltaTime);
                transform.LookAt(_shipTransform.position);
            }
            else
            {
                transform.position = _shipTransform.TransformPoint(_settings.CockpitOffset);
                transform.rotation = _shipTransform.rotation;
            }
        }

        // Exposed for testing
        public bool IsInCockpitMode => _currentMode == CameraMode.Cockpit;

        public void ToggleForTest() => OnToggle(default);
    }
}
