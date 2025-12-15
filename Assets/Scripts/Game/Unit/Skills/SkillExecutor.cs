using System;
using System.Collections.Generic;
using TowerDefence.Systems;
using UnityEngine;

public class SkillExecutor : MonoBehaviour
{
    [Header("Skills")]
    [SerializeField] private SkillData[] skillDatas;

    [Header("Projectile (only for skills that need it)")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileRoot;

    private ObjectPooler _pooler;
    private ProjectileSpawner _projectileSpawner;

    private readonly Dictionary<SkillTypeEnum, ISkill> _skills = new();

    private void Awake()
    {
        InitInfrastructure();
        InitSkills();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        foreach (var skill in _skills.Values)
            skill.Tick(dt);
    }

    private void InitInfrastructure()
    {
        _pooler = new ObjectPooler();
        _pooler.Init();

        _projectileSpawner = new ProjectileSpawner(
            _pooler,
            projectilePrefab,
            projectileRoot
        );
    }

    private void InitSkills()
    {
        foreach (var data in skillDatas)
        {
            ISkill skill = CreateSkill(data);

            // ін'єкція залежностей
            if (skill is ShootSkill shootSkill)
                shootSkill.InitSpawner(_projectileSpawner);

            _skills[data.Type] = skill;
        }
    }

    private ISkill CreateSkill(SkillData data)
    {
        return data.Type switch
        {
            SkillTypeEnum.Shoot => new ShootSkill(data),
            SkillTypeEnum.Dash => new DamageDashSkill(data),
            SkillTypeEnum.WideMelee => new WideMeleeSkill(data),
            SkillTypeEnum.AoE => new DamageAoESkill(data),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public bool TryUse(
        SkillTypeEnum skillType,
        Vector3? targetPosition,
        GameObject target = null
    )
    {
        if (!_skills.TryGetValue(skillType, out var skill))
            return false;

        var context = new SkillContext
        {
            Caster = gameObject,
            Target = target,
            CasterPosition = transform.position,
            TargetPosition = targetPosition
        };

        skill.Use(context);
        return true;
    }

    public float GetRange(SkillTypeEnum skillType)
    {
        return _skills.TryGetValue(skillType, out var skill)
            ? skill.Range
            : 0f;
    }
}
