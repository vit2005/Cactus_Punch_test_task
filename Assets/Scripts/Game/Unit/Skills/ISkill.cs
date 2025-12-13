public interface ISkill
{
    SkillTypeEnum Type { get; }

    float Range { get; }
    float Cooldown { get; }

    bool IsReady { get; }

    bool CanUse(SkillContext context);
    void Use(SkillContext context);
    void Tick(float deltaTime);
}
