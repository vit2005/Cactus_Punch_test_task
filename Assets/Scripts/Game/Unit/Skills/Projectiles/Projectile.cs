using TowerDefence.Systems;
using UnityEngine;

[RequireComponent(typeof(ProjectileMover))]
public class Projectile : MonoBehaviour
{
    private float _damage;
    private GameObject _damageOwner;
    private IObjectPooler _pooler;
    private string _poolKey;

    private ProjectileMover _mover;
    private TeamSide? _teamToIgnore;

    private void Awake()
    {
        _mover = GetComponent<ProjectileMover>();
    }

    public void Init(
        float damage,
        GameObject damageOwner,
        IObjectPooler pooler,
        string poolKey)
    {
        _damage = damage;
        _damageOwner = damageOwner;
        _pooler = pooler;
        _poolKey = poolKey;

        if (damageOwner.TryGetComponent<TeamMember>(out TeamMember teamMember))
        {
            _teamToIgnore = teamMember.Team;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == _damageOwner.name) return;

        if (other.TryGetComponent<HealthProvider>(out var healthProvider))
        {
            if (_teamToIgnore.HasValue &&
                other.TryGetComponent<TeamMember>(out var teamMember) &&
                teamMember.Team == _teamToIgnore.Value)
                return;

            healthProvider.ApplyDamage(_damage, _damageOwner);
        }

        Despawn();
    }

    private void OnEnable()
    {
        _mover.OnMaxDistanceReached += Despawn;
    }

    private void OnDisable()
    {
        _mover.OnMaxDistanceReached -= Despawn;
    }

    private void Despawn()
    {
        if (_damageOwner.name == "Player")
        {
            Debug.Log("Despawn");
        }
        _pooler.Release(_poolKey, this);
    }
}
