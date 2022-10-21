using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace RaceManager.Cars
{
    //[System.Serializable]
    [CreateAssetMenu(menuName = "Cars/CarConfigVisual", fileName = "CarNameVisualConfig", order = 1)]
    public class CarConfigVisual
	{

		public Dictionary<PartType, PartSet> WheelsSets; 
		
	}
}
