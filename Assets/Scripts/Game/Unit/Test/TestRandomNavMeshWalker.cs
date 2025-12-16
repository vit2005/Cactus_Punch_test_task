using UnityEngine;
using UnityEngine.AI;

public class TestRandomNavMeshWalker : MonoBehaviour
{
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private float waitTime = 2f;

    private NavMeshAgent _agent;
    private float _timer;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= waitTime && !_agent.pathPending && _agent.remainingDistance <= 0.5f)
        {
            Vector3 randomPoint = GetRandomPoint(transform.position, wanderRadius);
            _agent.SetDestination(randomPoint);
            _timer = 0f;
        }
    }

    private Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++) // 10 спроб знайти валідну точку
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }

        return center;
    }
}
