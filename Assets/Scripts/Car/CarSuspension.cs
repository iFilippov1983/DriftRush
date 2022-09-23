using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSuspension : MonoBehaviour
{
    [SerializeField] private Rigidbody _carRB;
    [SerializeField] private float _springForce = 2;
    [SerializeField] private float _springDamper = 0.5f;
    [SerializeField] private float _suspentionRestLength = 0.25f;
    [SerializeField] private List<Transform> _suspentionTargets;

    // Update is called once per frame
    void Update()
    {
        Suspend();
    }

    private void Suspend()
    {
        foreach (var tireTransform in _suspentionTargets)
        {
            RaycastHit tireRay;
            bool rayDidHit = Physics.Raycast(tireTransform.position, Vector3.down, out tireRay);//, _suspentionLegth, LayerMask.NameToLayer("Road"));

            if (rayDidHit)
            {
                Vector3 springDir = tireTransform.up;
                Vector3 tireWorldLevel = _carRB.GetPointVelocity(tireTransform.position);
                float offset = _suspentionRestLength - tireRay.distance;
                float vel = Vector3.Dot(springDir, tireWorldLevel);
                float force = (offset * _springForce) - (vel * _springDamper);

                _carRB.AddForceAtPosition(springDir * force, tireTransform.position);
                //if (hit.distance > _suspentionLegth)
                //    return;
                //float force = _forceAmount * (hit.distance / _suspentionLegth);
                //_carRB.AddForceAtPosition(target.up * force, target.transform.position);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Color color = Gizmos.color;
        Gizmos.color = Color.green;
        foreach (var t in _suspentionTargets)
        {
            Gizmos.DrawLine(t.position, new Vector3(t.position.x, t.position.y -_suspentionRestLength, t.position.z));
        }
    }
}
