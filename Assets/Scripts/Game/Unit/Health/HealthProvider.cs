using UnityEngine;
using static Unity.VisualScripting.Member;

public class HealthProvider : MonoBehaviour, IHealthProvider
{
    [SerializeField] private HealthData _data;

    public bool IsAlive => _data.IsAlive;

    public void ApplyDamage(float damage, GameObject source)
    {
        _data.ApplyDamage(damage);

        if (!IsAlive) Die(source);
    }

    public void ApplyHeal(float amount)
    {
        if (!IsAlive) return;

        _data.ApplyHeal(amount);
    }

    public void Revive(float hp)
    {
        _data.HP = _data.MaxHP;
    }

    private void Die(GameObject source)
    {
        
    }
}