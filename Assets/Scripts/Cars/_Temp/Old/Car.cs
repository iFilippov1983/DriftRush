using RaceManager.Cars;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Alt
{
    public class Car : MonoBehaviour
    {
        [ShowInInspector, ReadOnly]
        private string _id;

        [InfoBox("First two wheels must be Front wheels")]
        [SerializeField] private WheelCollider[] _wheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] _wheelMeshes = new GameObject[4];
        [SerializeField] private Vector3 _rbCenterOfMass;
        private CarSelfRighting _carSelfRighting;

        public Transform groundTrigger;
        public LayerMask wheelCollidables;

        public string ID => _id;
        public WheelCollider[] WheelColliders => _wheelColliders;
        public GameObject[] WheelMeshes => _wheelMeshes;
        public CarSelfRighting CarSelfRighting => _carSelfRighting;
        public Vector3 CenterOfMass => _rbCenterOfMass;


        private void OnEnable()
        {
            _id = MakeId();
            _carSelfRighting = GetComponent<CarSelfRighting>();
            //_carSelfRighting.Setup(_wheelColliders);
        }

        private string MakeId()
        {
            StringBuilder builder = new StringBuilder();
            Enumerable
               .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));
            return builder.ToString();
        }

        public bool WheelsGrounded()
        {
            return Physics.OverlapBox(groundTrigger.position, groundTrigger.localScale / 2, Quaternion.identity, wheelCollidables).Length > 0;
        }
    }
}
