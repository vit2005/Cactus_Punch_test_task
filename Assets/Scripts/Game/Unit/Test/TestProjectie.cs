using UnityEngine;

public class TestProjectie : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HealthProvider>(out var healthProvider))
        {
            Debug.Log($"Projectile hit an object [{other.name}], applying damage.");
            healthProvider.ApplyDamage(10f, null);
        }
    }
}
