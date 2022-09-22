using UnityEngine;
using TMPro;

public class SpeedIndicatorView : MonoBehaviour
{
    [SerializeField] private TMP_Text _speedValueText;

    public TMP_Text SpeedValueText => _speedValueText;
}
