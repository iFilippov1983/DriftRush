using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine;

namespace RaceManager.UI
{
    [Serializable]
    public class ChestSlot : MonoBehaviour
    {
        public Button slotButton;
        public TMP_Text timerText;
        public TMP_Text topText;
        public TMP_Text midText;
        public TMP_Text bottomText;
        public Image chestImage;
    }
}

