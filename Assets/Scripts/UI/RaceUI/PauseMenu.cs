using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RaceManager.Tools;
using RaceManager.Root;
using RaceManager.Race;
using UnityEngine.Events;

namespace RaceManager.UI
{
    public class PauseMenu : MonoBehaviour
    {
        private bool _gameIsPaused = false;

        [SerializeField] private Button _pauseButton;
        [SerializeField] private PauseMenuView _pauseMenuView;

        private void Awake()
        {
            _pauseButton.onClick.AddListener(Pause);
            _pauseMenuView.MenuButton.onClick.AddListener(LoadMenu);
            _pauseMenuView.RestartButton.onClick.AddListener(Restart);
            _pauseMenuView.ResumeButton.onClick.AddListener(Resume);
        }

        private void OnDestroy()
        {
            _pauseButton.onClick.RemoveAllListeners();
            _pauseMenuView.MenuButton.onClick.RemoveAllListeners();
            _pauseMenuView.RestartButton.onClick.RemoveAllListeners();
            _pauseMenuView.ResumeButton.onClick.RemoveAllListeners();
        }

        void Update()
        {
#if !MOBILE_INPUT
		if(Input.GetKeyUp(KeyCode.Escape))
		{
            CheckPause();
		}
#endif
        }

        private void CheckPause()
        {
            if (_gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        private void Pause()
        {
            _pauseMenuView.gameObject.SetActive(true);
            Time.timeScale = 0f;
            RaceEventsHub.BroadcastNotification(RaceEventType.PAUSE);
            _gameIsPaused = true;
        }

        private void Resume()
        {
            _pauseMenuView.gameObject.SetActive(false);
            Time.timeScale = 1f;
            RaceEventsHub.BroadcastNotification(RaceEventType.UNPAUSE);
            _gameIsPaused = false;
        }

        private void LoadMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneName.Scene_Menu);
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneName.Scene_Game);
        }
    }
}

