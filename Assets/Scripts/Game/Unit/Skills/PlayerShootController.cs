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

        Vector3 targetPos = transform.position + transform.forward * range;

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

        _fire = true;
    }



    public void OnHold(InputAction.CallbackContext context)
    {
        Vector2 screenPos = GetPointerPosition();
        if (!IsAttackSide(screenPos))
            return;

        if (context.performed)
            _fire = true;
    }


    public void OnTouchDelta(InputAction.CallbackContext context)
    {
        if (!_isAiming)
            return;

        // тут можна оновлювати UI прицілу
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
