using TowerDefence.Systems;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float _damage;
    private GameObject _damageOwner;
    private IObjectPooler _pooler;
    private string _poolKey;

    public void Init(
        float damage,
        GameObject damageOwner,
        IObjectPooler pooler,
        string poolKey)
    {
        _damage = damage;
        _damageOwner = damageOwner;
        _pooler = pooler;
        _poolKey = poolKey;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HealthProvider>(out var healthProvider))
        {
            healthProvider.ApplyDamage(_damage, _damageOwner);
        }

        Despawn();
    }

    private void Despawn()
    {
        _pooler.Release(_poolKey, this);
    }
}
