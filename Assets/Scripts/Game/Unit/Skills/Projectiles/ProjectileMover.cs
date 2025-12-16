using System;
using UnityEngine;

public class ProjectileMover : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;

    private bool _active;
    private float _maxDistance;
    private Vector3 _startPosition;

    public event Action OnMaxDistanceReached;

    public void Launch(float maxDistance)
    {
        _maxDistance = maxDistance;
        _startPosition = transform.position;
        _active = true;
    }

    private void Update()
    {
        if (!_active) return;

        transform.position += transform.forward * _speed * Time.deltaTime;

        if (Vector3.Distance(_startPosition, transform.position) >= _maxDistance)
        {
            _active = false;
            OnMaxDistanceReached?.Invoke();
        }
    }

    private void OnDisable()
    {
        _active = false;
    }
}
