using UnityEngine;

public class DamageAoESkill : BaseSkill
{
    public DamageAoESkill(SkillData data) : base(data)
    {
    }

    protected override void OnUse(SkillContext context)
    {
        var hits = Physics.OverlapSphere(context.CasterPosition, Data.Radius);
        foreach (var hit in hits)
        {
            if (hit.gameObject == context.Caster) continue;

            if (hit.TryGetComponent<HealthProvider>(out var hp))
            {
                hp.ApplyDamage(Data.Damage, context.Caster);
            }
        }
    }
}
