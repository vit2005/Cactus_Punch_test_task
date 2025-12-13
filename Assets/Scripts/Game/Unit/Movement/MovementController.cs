using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour, IMovementController
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _enabled = true;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    public void Move(Vector3 direction)
    {
        if (!_enabled)
            return;

        direction.y = 0f;

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        Vector3 move = direction * _moveSpeed;

        // гравітація (навіть у top-down вона потрібна)
        if (_controller.isGrounded)
            _velocity.y = 0f;

        _velocity.y += _gravity * Time.deltaTime;

        _controller.Move((move + _velocity) * Time.deltaTime);
    }

    public void Stop()
    {
        _velocity = Vector3.zero;
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;

        if (!enabled)
            Stop();
    }
}
