using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShootController : MonoBehaviour, Input.IGameActions
{
    [SerializeField] private float _shootCooldown = 0.5f;

    private PlayerInputController _playerInput;
    private SkillExecutor _skillExecutor;
    private Input _input;
    private float _lastShootTime;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInputController>();
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

    private void Shoot()
    {
        if (Time.time - _lastShootTime < _shootCooldown)
            return;

        _lastShootTime = Time.time;

        Vector3 targetPosition = _playerInput.GetAimPosition();
        _skillExecutor.TryUse(SkillTypeEnum.Shoot, targetPosition);
    }

#if UNITY_ANDROID || UNITY_IOS
    private bool IsAttackSide(Vector2 screenPosition)
    {
        // Left side of screen is for shooting
        return screenPosition.x < Screen.width * 0.5f;
    }
#endif

    // ===== Input callbacks =====

    public void OnTap(InputAction.CallbackContext context)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (context.performed)
        {
            Vector2 tapPosition = _input.Game.TouchPosition.ReadValue<Vector2>();
            
            if (IsAttackSide(tapPosition))
            {
                Shoot();
            }
        }
#elif UNITY_STANDALONE || UNITY_EDITOR
        if (context.performed)
        {
            Shoot();
        }
#endif
    }

    // Unused callbacks
    public void OnTouchDelta(InputAction.CallbackContext context) { }
    public void OnTouchPosition(InputAction.CallbackContext context) { }
    public void OnTouchPhase(InputAction.CallbackContext context) { }
    public void OnHold(InputAction.CallbackContext context) { }
    public void OnMove(InputAction.CallbackContext context) { }
}