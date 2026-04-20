using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

namespace Ochlophobia.Player
{
    /// Physics-based locomotion for XR. Moves the XR Origin via Rigidbody so
    /// wall/NPC colliders actually block movement.
    /// Attach to XR Origin (VR).
    [RequireComponent(typeof(Rigidbody))]
    public class XRPhysicsLocomotion : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private InputActionReference moveAction;

        private Rigidbody _rb;
        private XROrigin _xrOrigin;
        private Transform _cameraTransform;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _xrOrigin = GetComponent<XROrigin>();
        }

        private void Start()
        {
            _cameraTransform = _xrOrigin != null ? _xrOrigin.Camera.transform : Camera.main.transform;
            moveAction?.action.Enable();
        }

        private void FixedUpdate()
        {
            Vector2 input = Vector2.zero;

            // Joystick input via InputAction
            if (moveAction?.action != null)
                input = moveAction.action.ReadValue<Vector2>();

            // Keyboard fallback (for editor testing)
            if (input == Vector2.zero)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.wKey.isPressed) input.y += 1f;
                    if (Keyboard.current.sKey.isPressed) input.y -= 1f;
                    if (Keyboard.current.aKey.isPressed) input.x -= 1f;
                    if (Keyboard.current.dKey.isPressed) input.x += 1f;
                }
            }

            if (input.sqrMagnitude < 0.01f)
            {
                // Kill horizontal velocity when no input
                var v = _rb.linearVelocity;
                v.x = 0f;
                v.z = 0f;
                _rb.linearVelocity = v;
                return;
            }

            // Direction based on camera yaw (ignore pitch/roll)
            float yaw = _cameraTransform.eulerAngles.y;
            Quaternion yawRot = Quaternion.Euler(0, yaw, 0);
            Vector3 dir = yawRot * new Vector3(input.x, 0, input.y);

            Vector3 targetVelocity = dir.normalized * moveSpeed;
            targetVelocity.y = _rb.linearVelocity.y; // keep gravity
            _rb.linearVelocity = targetVelocity;
        }

        private void OnDestroy()
        {
            moveAction?.action.Disable();
        }
    }
}
