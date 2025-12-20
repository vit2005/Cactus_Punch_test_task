using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, Input.IGameActions
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    private Input _input;
    private IMovementController _movement;

#if UNITY_STANDALONE || UNITY_EDITOR
    private Camera _mainCamera;
#endif

    private Vector2 _moveDelta;

    private void Awake()
    {
        _movement = GetComponent<IMovementController>();

        _input = new Input();
        _input.Game.SetCallbacks(this);

#if UNITY_STANDALONE || UNITY_EDITOR
        _mainCamera = Camera.main;
#endif
    }

    private void OnEnable()
    {
        _input.Game.Enable();
    }

    private void OnDisable()
    {
        _input.Game.Disable();
    }

    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        // Mobile: touch drag on right side for movement
        Vector3 dir = new Vector3(_moveDelta.x, 0f, _moveDelta.y);

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();
            _movement.Move(dir);
        }
        else
        {
            _movement.Stop();
        }
#elif UNITY_STANDALONE || UNITY_EDITOR
        // Desktop: WASD/Arrow keys for movement
        Vector3 dir = new Vector3(_moveDelta.x, 0f, _moveDelta.y);

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();
            _movement.Move(dir);
        }
        else
        {
            _movement.Stop();
        }

        // Desktop: Rotate player to face mouse cursor
        RotateTowardsMouse();
#endif
    }

#if UNITY_STANDALONE || UNITY_EDITOR
    private void RotateTowardsMouse()
    {
        if (_mainCamera == null) return;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, _navMeshAgent.nextPosition);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            Vector3 direction = worldPoint - _navMeshAgent.nextPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }
        }
    }
#endif

    // ===== Input callbacks =====

    public void OnTouchDelta(InputAction.CallbackContext context)
    {
#if UNITY_ANDROID || UNITY_IOS
        _moveDelta = context.ReadValue<Vector2>();
#endif
    }

    public void OnTouchPosition(InputAction.CallbackContext context) { }
    public void OnTouchPhase(InputAction.CallbackContext context) { }
    public void OnTap(InputAction.CallbackContext context) { }
    public void OnHold(InputAction.CallbackContext context) { }

    public void OnMove(InputAction.CallbackContext context)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        _moveDelta = context.ReadValue<Vector2>();
#endif
    }

    public Vector3 GetAimPosition()
    {
#if UNITY_ANDROID || UNITY_IOS
        // Mobile: shoot in facing direction
        return transform.forward;
#elif UNITY_STANDALONE || UNITY_EDITOR
        // Desktop: shoot towards mouse
        if (_mainCamera == null) return transform.forward;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, _navMeshAgent.nextPosition);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            worldPoint.y = _navMeshAgent.nextPosition.y;
            Debug.Log("transform.position = " + transform.position.ToString() + 
                ", navMeshAgentPosition = " + _navMeshAgent.nextPosition.ToString());
            return worldPoint;
        }

        return Vector3.zero;
#else
        return _transform.forward;
#endif
    }
}