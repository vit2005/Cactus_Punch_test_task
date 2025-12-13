using UnityEngine;

public class WideMeleeSkill : BaseSkill
{
    public WideMeleeSkill(SkillData data) : base(data)
    {
    }

    protected override void OnUse(SkillContext context)
    {
        var hits = Physics.OverlapSphere(context.CasterPosition, Range);
        foreach (var hit in hits)
        {
            if (hit.gameObject == context.Caster) continue;

            if (hit.TryGetComponent<HealthProvider>(out var hp))
            {
                // перевіряємо кут
                Vector3 dir = (hit.transform.position - context.CasterPosition).normalized;
                if (Vector3.Angle(context.Caster.transform.forward, dir) <= Data.ConeAngle / 2)
                {
                    hp.ApplyDamage(Data.Damage, context.Caster);
                }
            }
        }
    }
}
