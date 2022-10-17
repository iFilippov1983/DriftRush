using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.Root
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        public Button StartButton => _startButton;
    }
}

