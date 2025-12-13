using System;
using System.Threading.Tasks;
using TowerDefence.Core;
using UnityEngine;

public class DamageDashSkill : BaseSkill
{
    private float _distanceTraveled = 0f;

    public DamageDashSkill(SkillData data) : base(data)
    {
    }

    protected override void OnUse(SkillContext context)
    {
        // лямбда "замкне" context
        Action<float> dashAction = null;
        dashAction = deltaTime =>
        {
            DashUpdate(context, deltaTime, dashAction);
        };

        Services.Get<ITickDispatcher>().Subscribe(dashAction, TickType.Update);
    }

    private void DashUpdate(SkillContext context, float deltaTime, Action<float> self)
    {
        float step = Data.Speed * deltaTime;
        context.Caster.transform.position += context.Caster.transform.forward * step;

        var hits = Physics.OverlapSphere(context.Caster.transform.position, 0.5f);
        foreach (var hit in hits)
        {
            if (hit.gameObject == context.Caster) continue;
            if (hit.TryGetComponent<HealthProvider>(out var hp))
            {
                hp.ApplyDamage(Data.Damage, context.Caster);
            }
        }

        _distanceTraveled += step;

        if (_distanceTraveled >= Range)
        {
            _distanceTraveled = 0f;
            Services.Get<ITickDispatcher>().Unsubscribe(self, TickType.Update);
        }
    }

}
