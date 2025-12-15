using UnityEngine;

[RequireComponent(typeof(SkillExecutor))]
public class EnemyShootController : MonoBehaviour
{
    [Header("Skill")]
    [SerializeField] private SkillTypeEnum shootSkillType = SkillTypeEnum.Shoot;

    [Header("Targeting")]
    [SerializeField] private TeamSide team;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float thinkInterval = 0.2f;

    private SkillExecutor _skillExecutor;
    private float _thinkTimer;

    private void Awake()
    {
        _skillExecutor = GetComponent<SkillExecutor>();
    }

    private void Update()
    {
        _thinkTimer -= Time.deltaTime;
        if (_thinkTimer > 0f)
            return;

        _thinkTimer = thinkInterval;

        Transform target = FindNearestEnemy();
        if (target == null)
            return;

        _skillExecutor.TryUse(
            shootSkillType,
            target.position,
            target.gameObject
        );
    }

    private Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            targetMask
        );

        Transform bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out TeamMember member))
                continue;

            if (member.Team == team)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                hit.transform.position
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}
