using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SkillExecutor))]
public class PlayerShootController : MonoBehaviour, Input.IGameActions
{
    [SerializeField] private SkillTypeEnum shootSkillType = SkillTypeEnum.Shoot;

    private SkillExecutor _skillExecutor;
    private Input _input;

    private bool _fire;
    private bool _isAiming;

    private Vector2 _aimDir;
    private Vector2 _aimStartScreenPos;

    private void Awake()
    {
        _skillExecutor = GetComponent<SkillExecutor>();

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
        if (!_fire)
            return;

        float range = _skillExecutor.GetRange(shootSkillType);

        Vector3 dir = new Vector3(_aimDir.x, 0f, _aimDir.y);
        if (dir.sqrMagnitude < 0.001f)
            dir = transform.forward;

        Vector3 targetPos = transform.position + dir.normalized * range;

        _skillExecutor.TryUse(shootSkillType, targetPos);

        _fire = false;
    }

    #region Input Callbacks

    public void OnTap(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Vector2 screenPos = GetPointerPosition();
        if (!IsAttackSide(screenPos))
            return;

        // швидкий постр≥л Ч у forward
        _aimDir = Vector2.zero;
        _fire = true;
    }

    public void OnHold(InputAction.CallbackContext context)
    {
        Vector2 screenPos = GetPointerPosition();
        if (!IsAttackSide(screenPos))
            return;

        if (context.started)
        {
            _isAiming = true;
            _aimStartScreenPos = screenPos;
            _aimDir = Vector2.zero;
            // тут можна включити UI приц≥лу
        }

        if (context.canceled && _isAiming)
        {
            _isAiming = false;
            _fire = true;
            // тут можна сховати UI приц≥лу
        }
    }

    public void OnTouchDelta(InputAction.CallbackContext context)
    {
        if (!_isAiming)
            return;

        Vector2 delta = context.ReadValue<Vector2>();
        if (delta.sqrMagnitude < 0.01f)
            return;

        _aimDir = delta.normalized;
        // тут можна оновлювати UI приц≥лу
    }

    public void OnMove(InputAction.CallbackContext context) { }
    public void OnTouchPhase(InputAction.CallbackContext context) { }
    public void OnTouchPosition(InputAction.CallbackContext context) { }

    #endregion

    #region Helpers

    private bool IsAttackSide(Vector2 screenPos)
    {
        return screenPos.x < Screen.width * 0.5f;
    }

    private Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        return Mouse.current.position.ReadValue();
    }

    #endregion
}
