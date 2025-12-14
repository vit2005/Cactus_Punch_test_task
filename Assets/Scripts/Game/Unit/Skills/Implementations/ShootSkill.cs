public class ShootSkill : BaseSkill
{
    private ProjectileSpawner _spawner;

    public ShootSkill(SkillData data) : base(data)
    {
    }

    public void InitSpawner(ProjectileSpawner spawner)
    {
         _spawner = spawner;
    }

    protected override void OnUse(SkillContext context)
    {
        _spawner.Spawn(
            context.CasterPosition,
            context.TargetPosition,
            Data.Damage,
            context.Caster
        );
    }
}
