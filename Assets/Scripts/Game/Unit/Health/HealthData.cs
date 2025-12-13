using UnityEngine;

public class HealthData
{
    public float HP;
    public float MaxHP;

    public float Armor;
    public float MaxArmor;

    public bool IsAlive => HP > 0;

    public float ApplyDamage(float damage)
    {
        var remainingDamage = damage;

        if (Armor > 0)
        {
            var absorbed = Mathf.Min(Armor, remainingDamage);
            Armor -= absorbed;
            remainingDamage -= absorbed;
        }

        HP -= remainingDamage;
        HP = Mathf.Max(HP, 0);

        return HP;
    }

    public void ApplyHeal(float amount)
    {
        HP = Mathf.Min(HP + amount, MaxHP);
    }
}
