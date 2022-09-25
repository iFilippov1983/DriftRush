using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ValuesSettings/CarDriverSettings", fileName = "DriverSet_Name")]
public class CarDriverSettings : ScriptableObject
{
    public float ReachedTargetDistance = 1f;//5
    public float StoppingDistance = 1f;//10
    public float StoppingSpeedLimit = 80f;
    public float StoppingSpeedLimitFinal = 15f;
    public float AngleCritical = 45f;
    public float ReverseDistance = 5f;
    public float StuckDistance = 0.6f;
    public float MaxStuckTime = 2f;
}
