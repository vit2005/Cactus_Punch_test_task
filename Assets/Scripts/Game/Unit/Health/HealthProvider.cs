using System;
using UnityEngine;

public class HealthProvider : MonoBehaviour, IHealthProvider
{
    private HealthData _data;

    public event Action<GameObject> OnDeath;

    public bool IsAlive => _data.IsAlive;
    public float HP => (_data.HP / _data.MaxHP) * 100f;

    public void Init(HealthData data)
    {
        _data = data;
    }

    public void ApplyDamage(float damage, GameObject source)
    {
        if (!IsAlive) return;

        _data.ApplyDamage(damage);

        if (!IsAlive) Die(source);
    }

    public void ApplyHeal(float amount)
    {
        if (!IsAlive) return;

        _data.ApplyHeal(amount);
    }

    public void Revive(float? hp = null)
    {
        _data.HP = hp ?? _data.MaxHP;
    }

    private void Die(GameObject source)
    {
        OnDeath?.Invoke(source);
    }
}