using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Cars
{
    [RequireComponent(typeof(Car))]
    public class CarVisual : MonoBehaviour
    {
        private Car _car;
        [SerializeField] private CarConfigVisual _carConfigVisual;

        [OnInspectorGUI]
        private void Awake()
        {
            _car = GetComponent<Car>();

        }
    }

    public class Part : MonoBehaviour
    { 
       
    }
}
