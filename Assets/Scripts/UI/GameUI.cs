using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private SpeedIndicatorView _speedIndicatorView;
    [SerializeField] private CarEngine _carDriver;

    private void OnGUI()
    {
        int value = Mathf.RoundToInt(_carDriver.Speed);
        _speedIndicatorView.SpeedValueText.text = value.ToString();
    }
}
