using System.Collections;
using TowerDefence.Systems;
using UnityEngine;

public class TestProjectileSpawner : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileRoot;

    [Header("Shoot settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float shootInterval = 0.5f;
    [SerializeField] private int shotsCount = 5;

    [Header("Target (optional)")]
    [SerializeField] private Transform target;

    private ObjectPooler _pooler;
    private ProjectileSpawner _projectileSpawner;

    private void Start()
    {
        InitSystems();
        StartCoroutine(ShootRoutine());
    }

    private void InitSystems()
    {
        _pooler = new ObjectPooler();
        _pooler.Init();

        _projectileSpawner = new ProjectileSpawner(
            _pooler,
            projectilePrefab,
            projectileRoot
        );
    }

    private IEnumerator ShootRoutine()
    {
        for (int i = 0; i < shotsCount; i++)
        {
            Shoot();
            yield return new WaitForSeconds(shootInterval);
        }
    }

    private void Shoot()
    {
        Vector3? targetPosition = target != null
            ? target.position
            : null;

        _projectileSpawner.Spawn(
            transform.position,
            targetPosition,
            damage,
            gameObject
        );
    }
}
