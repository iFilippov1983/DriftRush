using UnityEngine;

namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Configs/CarConfigSO", fileName = "CarConfigSO_Name", order = 1)]
	public class CarConfigScriptable : ScriptableObject
	{
		[SerializeField] private CarConfig _carConfig;

		public CarConfig CarConfig => _carConfig;
	}
}
