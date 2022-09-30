using RaceManager.Tools;
using RaceManager.Vehicles;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class Finish : MonoBehaviour
    {
        private List<string> ids = new List<string>();
        private List<CarAI> _carAIs;

        private void Start()
        {
            _carAIs = new List<CarAI>(FindObjectsOfType<CarAI>());
        }

        private void OnTriggerEnter(Collider other)
        {
            CarAI otherCarAI;
            bool isCarAI = other.TryGetComponent<CarAI>(out otherCarAI);
            if (isCarAI)
            { 
                otherCarAI.StopEngine();
            }

            //Made in purpose to avoid multiple tiggering
            //CarAI carAI;
            //carAI = _carAIs.Find(c => c.GetID() == otherCarAI.GetID());
            
            //bool contains = false;
            //if (carAI != null)
            //{
            //    contains = ids.Contains(carAI.GetID());
            //    if (!contains)
            //    {
            //        ids.Add(carAI.GetID());
            //        carAI.StopEngine();
            //    }
            //}

            if (other.gameObject.tag == Literal.Tag_Player)
                RaceEventsHub.Notify(RaceEventType.FINISH);
        }

        private void OnDestroy()
        {
            ids.Clear();
        }
    }
}

