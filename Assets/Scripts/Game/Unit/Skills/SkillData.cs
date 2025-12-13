using System;
using UnityEngine;

public enum SkillTypeEnum 
{ 
    Shoot,
    Dash, 
    WideMelee, 
    AoE 
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public SkillTypeEnum Type;
    public float Range;
    public float Cooldown;
    public float Damage;
    public float Speed; // для Dash, якщо потрібно
    public float Radius; // для AoE
    public float ConeAngle; // для WideMelee
    // можна додати інші параметри, ProjectilePrefab тощо

    private BaseSkill CreateSkill(SkillData data)
    {
        return data.Type switch
        {
            SkillTypeEnum.Shoot => new ShootSkill(this),
            SkillTypeEnum.Dash => new DamageDashSkill(this),
            SkillTypeEnum.WideMelee => new WideMeleeSkill(this),
            SkillTypeEnum.AoE => new DamageAoESkill(this),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
