using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaceManager.Root
{
    public class MainUI : MonoBehaviour
    {
        [SerializeField] private MainMenu _mainMenu;

        private void Start()
        {
            _mainMenu.StartButton.onClick.AddListener(StartRace);
        }

        private void StartRace()
        {
            Loader.Load(Loader.Scene.RaceScene);
        }
    }
}

