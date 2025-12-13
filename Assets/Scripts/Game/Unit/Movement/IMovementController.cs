using UnityEngine;

public interface IMovementController
{
    void Move(Vector3 direction);
    void Stop();
    void SetEnabled(bool enabled);
}
