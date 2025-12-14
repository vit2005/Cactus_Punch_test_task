using UnityEngine;

public class ProjectileMover : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;

    private bool _active;

    public void Launch()
    {
        _active = true;
    }

    private void Update()
    {
        if (!_active) return;

        transform.position += transform.forward * _speed * Time.deltaTime;
    }

    private void OnDisable()
    {
        _active = false;
    }
}
