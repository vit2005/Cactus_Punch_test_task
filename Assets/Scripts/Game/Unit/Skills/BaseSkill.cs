using UnityEngine;

public abstract class BaseSkill : ISkill
{
    protected SkillData Data; // тут зберігаються всі параметри

    public virtual SkillTypeEnum Type => Data.Type;
    public virtual float Range => Data.Range;
    public virtual float Cooldown => Data.Cooldown;

    private float _cooldownRemaining;

    public bool IsReady => _cooldownRemaining <= 0f;

    public BaseSkill(SkillData data)
    {
        Data = data;
    }

    public virtual void Tick(float deltaTime)
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= deltaTime;
    }

    public virtual bool CanUse(SkillContext context)
    {
        if (!IsReady) return false;

        return true;

        //float distance = Vector3.Distance(
        //    context.CasterPosition,
        //    context.TargetPosition
        //);

        //return distance <= Range;
    }

    public void Use(SkillContext context)
    {
        if (!CanUse(context))
            return;

        OnUse(context);
        _cooldownRemaining = Cooldown;
    }

    protected abstract void OnUse(SkillContext context);
}
