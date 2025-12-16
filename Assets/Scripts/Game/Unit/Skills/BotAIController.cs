using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class BotAIController : MonoBehaviour
{
    private enum State
    {
        Idle,
        Wander,
        Attack
    }

    [Header("Movement")]
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Group Movement")]
    [SerializeField] private float allySearchRadius = 12f;
    [SerializeField] private float cohesionStrength = 0.4f;

    [Header("Vision")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float eyeHeight = 0f;
    [SerializeField] private float projectileWidth = 0.3f; // ширина кулі (діаметр)

    [Header("Combat")]
    [SerializeField] private SkillTypeEnum shootSkillType = SkillTypeEnum.Shoot;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float thinkInterval = 0.2f;
    [SerializeField] private float attackRange = 6f;

    private NavMeshAgent _agent;
    private SkillExecutor _skillExecutor;
    private TeamMember _teamMember;

    private float _timer;
    private float _thinkTimer;
    private Transform _currentTarget;

    private State _currentState = State.Idle;

    // Кеш для оптимізації
    private static Collider[] _sharedBuffer = new Collider[50];

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _skillExecutor = GetComponent<SkillExecutor>();
        _teamMember = GetComponent<TeamMember>();
    }

    private void Update()
    {
        // Думка про ворогів
        _thinkTimer -= Time.deltaTime;
        if (_thinkTimer <= 0f)
        {
            _thinkTimer = thinkInterval;
            _currentTarget = FindNearestEnemy();
        }

        // Стейт-логіка
        switch (_currentState)
        {
            case State.Idle:
                if (_currentTarget != null)
                {
                    _currentState = State.Attack;
                }
                else
                {
                    _currentState = State.Wander;
                    _timer = waitTime;
                }
                break;

            case State.Wander:
                _timer += Time.deltaTime;
                if (_currentTarget != null)
                {
                    _currentState = State.Attack;
                }
                else if (_timer >= waitTime && !_agent.pathPending && _agent.remainingDistance <= 0.5f)
                {
                    Vector3 targetPoint = GetGroupWanderPoint();
                    _agent.SetDestination(targetPoint);
                    _timer = 0f;
                }
                break;

            case State.Attack:
                if (_currentTarget == null || !HasLineOfSight(_currentTarget))
                {
                    _currentTarget = FindNearestEnemy();
                    if (_currentTarget == null)
                    {
                        _currentState = State.Wander;
                        break;
                    }
                }

                // Знаходимо всіх видимих ворогів
                var visibleEnemies = GetVisibleEnemies();

                if (visibleEnemies.Count == 0)
                {
                    _currentTarget = null;
                    _currentState = State.Wander;
                    break;
                }

                // Визначаємо позицію для руху
                Vector3 moveTarget = CalculateAttackPosition(visibleEnemies);
                _agent.SetDestination(moveTarget);

                // Якщо стоїмо - повертаємось до цілі
                if (_agent.velocity.sqrMagnitude < 0.1f && _currentTarget != null)
                {
                    RotateTowardsTarget(_currentTarget);
                }

                // Стріляємо в найближчого видимого ворога
                Transform closestEnemy = GetClosestEnemy(visibleEnemies);
                if (closestEnemy != null)
                {
                    _skillExecutor.TryUse(
                        shootSkillType,
                        closestEnemy.position,
                        closestEnemy.gameObject
                    );
                }
                break;
        }

        RotateTowardsMovement();
    }

    private List<Transform> GetVisibleEnemies()
    {
        var enemies = new List<Transform>();
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, _sharedBuffer);

        for (int i = 0; i < count; i++)
        {
            var hit = _sharedBuffer[i];

            // Пропускаємо себе
            if (hit.transform == transform)
                continue;

            // Перевіряємо команду
            if (!hit.TryGetComponent(out TeamMember member))
                continue;

            if (member.Team == _teamMember.Team)
                continue; // Ігноруємо союзників

            // Перевіряємо видимість
            if (!HasLineOfSight(hit.transform))
                continue;

            enemies.Add(hit.transform);
        }

        return enemies;
    }

    private Transform GetClosestEnemy(List<Transform> enemies)
    {
        if (enemies.Count == 0)
            return null;

        Transform closest = null;
        float bestDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    private Vector3 CalculateAttackPosition(List<Transform> enemies)
    {
        if (enemies.Count == 0)
            return transform.position;

        // Знаходимо найближчого ворога
        Transform closest = GetClosestEnemy(enemies);
        float distToClosest = Vector3.Distance(transform.position, closest.position);

        // Якщо далеко - підходимо ближче
        if (distToClosest > attackRange)
        {
            Vector3 dirToEnemy = (closest.position - transform.position).normalized;
            return transform.position + dirToEnemy * 2f;
        }
        // Якщо дуже близько - відступаємо
        else if (distToClosest < attackRange * 0.5f)
        {
            Vector3 dirAway = (transform.position - closest.position).normalized;
            return transform.position + dirAway * 1.5f;
        }
        // Оптимальна дистанція - залишаємось на місці
        else
        {
            return transform.position;
        }
    }

    private void RotateTowardsMovement()
    {
        Vector3 velocity = _agent.velocity;

        if (velocity.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void RotateTowardsTarget(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion rot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rot,
            rotationSpeed * Time.deltaTime
        );
    }

    private bool TryGetAlliesCenter(out Vector3 center)
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            allySearchRadius,
            _sharedBuffer
        );

        Vector3 sum = Vector3.zero;
        int allyCount = 0;

        for (int i = 0; i < count; i++)
        {
            var hit = _sharedBuffer[i];

            if (hit.transform == transform)
                continue;

            if (!hit.TryGetComponent(out TeamMember member))
                continue;

            if (member.Team != _teamMember.Team)
                continue;

            sum += hit.transform.position;
            allyCount++;
        }

        if (allyCount == 0)
        {
            center = Vector3.zero;
            return false;
        }

        center = sum / allyCount;
        return true;
    }

    private Vector3 GetGroupWanderPoint()
    {
        Vector3 randomPoint = GetRandomPoint(transform.position, wanderRadius);

        if (!TryGetAlliesCenter(out Vector3 alliesCenter))
            return randomPoint;

        Vector3 toRandom = randomPoint - transform.position;
        Vector3 toAllies = alliesCenter - transform.position;

        Vector3 blendedDirection = Vector3.Lerp(toRandom, toAllies, cohesionStrength);
        Vector3 finalPoint = transform.position + blendedDirection;

        if (NavMesh.SamplePosition(finalPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            return hit.position;

        return randomPoint;
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = target.position + Vector3.up * eyeHeight;

        Vector3 direction = (targetPos - origin);
        float distance = direction.magnitude;
        direction.Normalize();

        // Вектор вправо від напрямку пострілу
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

        float halfWidth = projectileWidth * 0.5f;

        Vector3 leftOrigin = origin - right * halfWidth;
        Vector3 rightOrigin = origin + right * halfWidth;

        bool leftHit = Physics.Raycast(
            leftOrigin,
            direction,
            distance,
            obstacleMask
        );

        bool rightHit = Physics.Raycast(
            rightOrigin,
            direction,
            distance,
            obstacleMask
        );

#if UNITY_EDITOR
        Debug.DrawLine(leftOrigin, leftOrigin + direction * distance, leftHit ? Color.red : Color.green, 0.1f);
        Debug.DrawLine(rightOrigin, rightOrigin + direction * distance, rightHit ? Color.red : Color.green, 0.1f);
#endif

        return !(leftHit || rightHit);
    }


    private Transform FindNearestEnemy()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, _sharedBuffer);

        Transform bestTarget = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var hit = _sharedBuffer[i];

            // Пропускаємо себе
            if (hit.transform == transform)
                continue;

            // Перевіряємо команду
            if (!hit.TryGetComponent(out TeamMember member))
                continue;

            if (member.Team == _teamMember.Team)
                continue; // Ігноруємо союзників!

            if (!HasLineOfSight(hit.transform))
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

    private Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }
        return center;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Малюємо радіус пошуку союзників
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, allySearchRadius);

        // Малюємо лінію до поточної цілі
        if (_currentTarget != null)
        {
            Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
            Vector3 targetEyePos = _currentTarget.position + Vector3.up * eyeHeight;

            bool hasLOS = HasLineOfSight(_currentTarget);
            Gizmos.color = hasLOS ? Color.green : Color.red;
            Gizmos.DrawLine(eyePos, targetEyePos);

            // Малюємо сферу на позиції очей
            Gizmos.DrawWireSphere(eyePos, 0.2f);
        }

        // Інфо про ObstacleMask
        if (_currentTarget != null)
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 3f,
                $"State: {_currentState}\nTarget: {_currentTarget.name}\nObstacleMask: {obstacleMask.value}"
            );
        }
    }
#endif
}