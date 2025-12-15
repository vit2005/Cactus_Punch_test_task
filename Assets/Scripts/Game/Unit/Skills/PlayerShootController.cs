using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SkillExecutor))]
public class PlayerShootController : MonoBehaviour, Input.IGameActions
{
    [SerializeField] private SkillTypeEnum shootSkillType = SkillTypeEnum.Shoot;

    private SkillExecutor _skillExecutor;
    private Input _input;
    private bool _fire;

    private void Awake()
    {
        _skillExecutor = GetComponent<SkillExecutor>();

        _input = new Input();
        _input.Game.SetCallbacks(this);
    }

    private void Update()
    {
        if (!_fire) return;

        float range = _skillExecutor.GetRange(shootSkillType);
        Vector3 targetPos = transform.position + transform.forward * range;

        _skillExecutor.TryUse(shootSkillType, targetPos);

        _fire = false;
    }

    #region Input Callbacks

    public void OnHold(InputAction.CallbackContext context) { }

    public void OnMove(InputAction.CallbackContext context) { }

    public void OnTap(InputAction.CallbackContext context) { _fire = true; }

    public void OnTouchDelta(InputAction.CallbackContext context) { }

    public void OnTouchPhase(InputAction.CallbackContext context) { }

    public void OnTouchPosition(InputAction.CallbackContext context) { }

    #endregion
}
