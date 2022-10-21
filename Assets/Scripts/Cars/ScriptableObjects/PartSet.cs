using UnityEngine;

namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Cars/PartSet", fileName = "CarName_PartSet", order = 1)]
    public class PartSet : ScriptableObject
	{ 
		public CarName CarName;
		public PartType PartType;
		public SetType SetType;
	}
}
