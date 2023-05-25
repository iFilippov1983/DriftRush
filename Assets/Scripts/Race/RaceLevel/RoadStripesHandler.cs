using UnityEngine;
using RaceManager.Effects;
using RaceManager.Cars;
using RaceManager.Root;
using System.Collections.Generic;
using UniRx.Triggers;
using UniRx;

namespace RaceManager.Race
{
    public class RoadStripesHandler : MonoBehaviour
    {
        [SerializeField] private HapticType _hapticTypeToPlay = HapticType.Light;
        [SerializeField] private List<Collider> _colliders = new List<Collider>();

        private void Awake()
        {
            foreach(var c in _colliders) 
            { 
                c.OnTriggerEnterAsObservable()
                    .Subscribe(other => 
                    {
                        if (other.TryGetComponent(out CarAI carAi))
                        {
                            if (carAi.PlayerDriving && CarIsSlipping(carAi.Car))
                            {
                                Singleton<GameEffectsController>.Instance.PlayEffect(_hapticTypeToPlay);
                                //$"[RoadStripe] HAPTIC - {_hapticTypeToPlay}".Log(Logger.ColorBlue);
                            }
                        }
                    })
                    .AddTo(this);
            }
        }

        private bool CarIsSlipping(Car car)
        {
            foreach (var w in car.Wheels)
            {
                if (w.HasSideSlip)
                    return true;
            }
            return false;
        }
    }
}