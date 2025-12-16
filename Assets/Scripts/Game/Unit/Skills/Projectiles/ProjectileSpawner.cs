using System;
using TowerDefence.Systems;
using UnityEngine;

public class ProjectileSpawner
{
    private const string ProjectilePoolKey = "Projectile";

    private readonly IObjectPooler _pooler;
    private readonly Projectile _projectilePrefab;
    private readonly Transform _projectileRoot;

    public ProjectileSpawner(
        IObjectPooler pooler,
        Projectile projectilePrefab,
        Transform projectileRoot)
    {
        _pooler = pooler;
        _projectilePrefab = projectilePrefab;
        _projectileRoot = projectileRoot;

        CreatePool();
    }

    private void CreatePool()
    {
        _pooler.CreatePool(
            ProjectilePoolKey,
            factory: () =>
            {
                var projectile = GameObject
                    .Instantiate(_projectilePrefab, _projectileRoot);

                projectile.gameObject.SetActive(false);
                return projectile;
            },
            onGet: p =>
            {
                p.gameObject.SetActive(true);
            },
            onRelease: p =>
            {
                p.gameObject.SetActive(false);
            },
            prewarmCount: 20
        );
    }

    public void Spawn(
        Vector3 casterPosition,
        Vector3? targetPosition,
        float damage,
        float range,
        GameObject damageOwner)
    {
        var projectile = _pooler.Get<Projectile>(ProjectilePoolKey);

        projectile.transform.position = casterPosition;

        if (targetPosition.HasValue)
        {
            var dir = (targetPosition.Value - casterPosition).normalized;
            projectile.transform.forward = dir;
        }

        projectile.Init(
            damage,
            damageOwner,
            _pooler,
            ProjectilePoolKey
        );

        projectile.GetComponent<ProjectileMover>()?.Launch(range);
    }
}
