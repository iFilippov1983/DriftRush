using UnityEngine;
using TMPro;

namespace RaceManager.UI
{
    public class PositionIndicatorView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _positionText;
        [SerializeField] private TMP_Text _driversTotalText;

        public TMP_Text PositionText => _positionText;
        public TMP_Text DriverTotalText => _driversTotalText;
    }
}

