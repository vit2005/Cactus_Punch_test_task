using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, Input.IGameActions
{
    private Input _input;
    private IMovementController _movement;

    private Vector2 _moveDelta;

    private void Awake()
    {
        _movement = GetComponent<IMovementController>();

        _input = new Input();
        _input.Game.SetCallbacks(this);
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
        // screen-space delta → world-space direction
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
    }

    // ===== Input callbacks =====

    public void OnTouchDelta(InputAction.CallbackContext context)
    {
        _moveDelta = context.ReadValue<Vector2>();
    }

    public void OnTouchPosition(InputAction.CallbackContext context) { }
    public void OnTouchPhase(InputAction.CallbackContext context) { }
    public void OnTap(InputAction.CallbackContext context) { }
    public void OnHold(InputAction.CallbackContext context) { }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDelta = context.ReadValue<Vector2>();
        Debug.Log($"Move input: {_moveDelta}"); // Debug log to verify input reception
    }
}
