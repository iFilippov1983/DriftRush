using UnityEngine;
using TMPro;

namespace RaceManager.UI
{
    public class PositionIndicatorView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _positionText;

        public TMP_Text PositionText => _positionText;
    }
}

