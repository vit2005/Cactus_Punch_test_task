using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerShootController : MonoBehaviour, Input.IGameActions
{
    [SerializeField] private float _shootCooldown = 0.5f;
    [SerializeField] private NavMeshAgent _navMeshAgent;

#if UNITY_ANDROID || UNITY_IOS
    [Header("Mobile Auto-Aim")]
    [SerializeField] private float _detectionRadius = 1000f;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _eyeHeight = 0f;
    [SerializeField] private float _projectileWidth = 0.3f;

    // Кеш для оптимізації
    private static Collider[] _sharedBuffer = new Collider[100];
#endif

    private PlayerInputController _playerInput;
    private SkillExecutor _skillExecutor;
    private TeamMember _teamMember;
    private Input _input;
    private float _lastShootTime;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInputController>();
        _skillExecutor = GetComponent<SkillExecutor>();
        _teamMember = GetComponent<TeamMember>();

        _input = new Input();
        _input.Game.SetCallbacks(this);
    }

    private void OnEnable()
    {
        _input.Game.Enable();
    }

    private void OnDisable()
    {
        _input.Game.Disable();
    }

    private void Shoot()
    {
        if (Time.time - _lastShootTime < _shootCooldown)
            return;

        _lastShootTime = Time.time;

#if UNITY_ANDROID || UNITY_IOS
        // Mobile: завжди стріляємо при тапі
        var target = FindBestTarget();
        if (target.HasValue)
        {
            _skillExecutor.TryUse(SkillTypeEnum.Shoot, target.Value.Position, target.Value.GameObject);
        }
#elif UNITY_STANDALONE || UNITY_EDITOR
        // Desktop: стрільба у напрямку миші
        Vector3 targetPosition = _playerInput.GetAimPosition();
        _skillExecutor.TryUse(SkillTypeEnum.Shoot, targetPosition);
#endif
    }

#if UNITY_ANDROID || UNITY_IOS
    private TargetInfo? FindBestTarget()
    {
        // Отримуємо всіх можливих ворогів
        int count = Physics.OverlapSphereNonAlloc(_navMeshAgent.nextPosition, _detectionRadius, _sharedBuffer);

        // Список усіх ворогів з відстанями
        List<EnemyDistance> enemies = new List<EnemyDistance>();

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

            // Отримуємо позицію ворога (NavMeshAgent якщо є, інакше Transform)
            Vector3 enemyPosition = hit.transform.position;
            if (hit.TryGetComponent(out NavMeshAgent enemyAgent))
            {
                enemyPosition = enemyAgent.nextPosition;
            }

            float distance = Vector3.Distance(_navMeshAgent.nextPosition, enemyPosition);
            enemies.Add(new EnemyDistance
            {
                GameObject = hit.gameObject,
                Distance = distance,
                Position = enemyPosition
            });
        }

        if (enemies.Count == 0)
            return null;

        // Сортуємо від найближчого до найдальшого
        enemies = enemies.OrderBy(e => e.Distance).ToList();

        // Шукаємо першого видимого ворога
        foreach (var enemy in enemies)
        {
            if (HasLineOfSight(enemy.Position))
            {
                return new TargetInfo
                {
                    Position = enemy.Position,
                    GameObject = enemy.GameObject
                };
            }
        }

        // Якщо всі за перепонами - стріляємо в найближчого
        return new TargetInfo
        {
            Position = enemies[0].Position,
            GameObject = enemies[0].GameObject
        };
    }

    private bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 origin = _navMeshAgent.nextPosition + Vector3.up * _eyeHeight;
        Vector3 targetPos = targetPosition + Vector3.up * _eyeHeight;

        Vector3 direction = (targetPos - origin);
        float distance = direction.magnitude;
        direction.Normalize();

        // Вектор вправо від напрямку пострілу
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

        float halfWidth = _projectileWidth * 0.5f;

        Vector3 leftOrigin = origin - right * halfWidth;
        Vector3 rightOrigin = origin + right * halfWidth;

        // Перевіряємо ліву і праву сторони снаряда
        bool leftHit = Physics.Raycast(
            leftOrigin,
            direction,
            distance,
            _obstacleMask
        );

        bool rightHit = Physics.Raycast(
            rightOrigin,
            direction,
            distance,
            _obstacleMask
        );

#if UNITY_EDITOR
        Debug.DrawLine(leftOrigin, leftOrigin + direction * distance, leftHit ? Color.red : Color.green, 0.1f);
        Debug.DrawLine(rightOrigin, rightOrigin + direction * distance, rightHit ? Color.red : Color.green, 0.1f);
#endif

        return !(leftHit || rightHit);
    }

    // Структура для зберігання інформації про ціль
    private struct TargetInfo
    {
        public Vector3 Position;
        public GameObject GameObject;
    }

    // Структура для зберігання ворога та відстані
    private struct EnemyDistance
    {
        public GameObject GameObject;
        public float Distance;
        public Vector3 Position;
    }
#endif

    // ===== Input callbacks =====

    public void OnTap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Shoot();
        }
    }

    // Unused callbacks
    public void OnTouchDelta(InputAction.CallbackContext context) { }
    public void OnTouchPosition(InputAction.CallbackContext context) { }
    public void OnTouchPhase(InputAction.CallbackContext context) { }
    public void OnHold(InputAction.CallbackContext context) { }
    public void OnMove(InputAction.CallbackContext context) { }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
#if UNITY_ANDROID || UNITY_IOS
        // Візуалізація радіусу автоприцілу
        if (_navMeshAgent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_navMeshAgent.nextPosition, _detectionRadius);
        }
#endif
    }
#endif
}