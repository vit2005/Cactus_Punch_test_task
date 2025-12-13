using UnityEngine;

public interface IHealthProvider
{
    bool IsAlive { get; }
    void ApplyDamage(float damage, GameObject source);
    void ApplyHeal(float amount);

    // bool CanRevive { get; } // Optional property to indicate if the unit can be revived
    void Revive(float hp);
}