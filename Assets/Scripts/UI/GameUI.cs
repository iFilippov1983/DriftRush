using RaceManager.Vehicles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private SpeedIndicatorView _speedIndicatorView;
        [SerializeField] private SpeedIndicatorView _speedIndicatorView_Opponent;
        [SerializeField] private CarController _carController;
        [SerializeField] private CarController _carOpponent;

        private void OnGUI()
        {
            int value = Mathf.RoundToInt(_carController.CurrentSpeed);
            _speedIndicatorView.SpeedValueText.text = value.ToString();

            value = Mathf.RoundToInt(_carOpponent.CurrentSpeed);
            _speedIndicatorView_Opponent.SpeedValueText.text = value.ToString();
        }
    }
}

