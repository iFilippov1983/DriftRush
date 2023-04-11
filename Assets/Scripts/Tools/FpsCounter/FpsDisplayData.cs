using TMPro;
using UnityEngine;

namespace RaceManager.Tools
{
    public struct FpsDisplayData
    {
        public TMP_Text averageText;
        public TMP_Text highestText;
        public TMP_Text lowestText;
        public int MaxFpsToDisplay; // 99
        public Color[] Colors;
    }
}
