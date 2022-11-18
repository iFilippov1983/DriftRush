using RaceManager.Progress;
using RaceManager.Tools;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class GameProgressPanel : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [Space]
        [SerializeField] private TMP_Text _moneyAmountText;
        [SerializeField] private TMP_Text _gemsAmountText;
        [Space]
        [SerializeField] private GridLayoutGroup _progressStepsContent;
        [SerializeField] private RectTransform _progressStepsContentRect;

        private GameObject _progressStepPrefab;
        private SpritesContainerCarCards _spritesContainer;
        private GameProgressScheme _gameProgressScheme;

        private List<ProgressStepView> _progressSteps = new List<ProgressStepView>();

        private GameObject ProgressStepPrefab
        {
            get 
            {
                if (_progressStepPrefab == null)
                    _progressStepPrefab = ResourcesLoader.LoadPrefab(ResourcePath.ProgressStepPrefab);

                return _progressStepPrefab;
            }
        }

        public Button BackButton => _backButton;

        [Inject]
        private void Construct(GameProgressScheme gameProgressScheme, SpritesContainerCarCards spritesContainer)
        { 
            _gameProgressScheme = gameProgressScheme;
            _spritesContainer = spritesContainer;
        }
    }
}

