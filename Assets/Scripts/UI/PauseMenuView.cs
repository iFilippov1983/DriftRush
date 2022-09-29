using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        public Button ResumeButton => _resumeButton;
        public Button RestartButton => _restartButton;
        public Button MenuButton => _menuButton;
    }
}

