using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class StartButtonView : AnimatableSubject
    {
        [Space]
        [Header("Main Fialds")]
        [SerializeField] private Button _button;

        public Button Button => _button;
    }
}

