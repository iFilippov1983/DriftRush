using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.Root
{
    public class MainUI : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private List<ChestSlot> _chestSlots;
        [SerializeField] private ChestProgressPanel _chestProgress;
        [SerializeField] private CupsProgressPanel _cupsProgress;
        [SerializeField] private CurrencyAmountPanel _currencyAmount;
        [SerializeField] private TuningPanel _tuningPanel;

        private void Start()
        {
            _startButton.onClick.AddListener(StartRace);
        }

        private void StartRace()
        {
            Loader.Load(Loader.Scene.RaceScene);
        }
    }
}

