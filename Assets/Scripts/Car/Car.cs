using RaceManager.Cars.Effects;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    public class Car : MonoBehaviour
    {
        [ShowInInspector, ReadOnly]
        private string _id;

        [InfoBox("First two wheels must be Front wheels")]
        [SerializeField] private WheelCollider[] _wheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] _wheelMeshes = new GameObject[4];
        private CarSelfRighting _carSelfRighting;

        public string ID => _id;
        public WheelCollider[] WheelColliders => _wheelColliders;
        public GameObject[] WheelMeshes => _wheelMeshes;
        public CarSelfRighting CarSelfRighting => _carSelfRighting;


        private void OnEnable()
        {
            _id = MakeId();
            _carSelfRighting = GetComponent<CarSelfRighting>();
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
    }
}
