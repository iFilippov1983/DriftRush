using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "CarSettings/CarSettings", fileName = "CarSet_Name")]
public class CarSettings : ScriptableObject
{
    [Title("Move options")]
    public float speedMax = 10f;
    public float speedMin = -50f;
    public float acceleration = 10f;
    public float reverseSpeed = 30f;
    [Title("Stop options")]
    public float brakeSpeed = 100f;
    public float idleSlowdown = 30f;
    [Title("Turn options")]
    public float turnSpeedMax = 300f;
    public float turnSpeedAcceleration = 300f;
    public float turnIdleSlowdown = 500f;
}
