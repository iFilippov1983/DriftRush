using UnityEngine;
using UnityEngine.Events;

public interface ICarDriver
{
    public UnityEvent TargetReachedEvent { get; set; }
    public void SetTargetPosition(Vector3 targetPosition);
}
