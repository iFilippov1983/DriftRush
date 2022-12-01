using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class ProgressStepView : MonoBehaviour
    {
        [SerializeField] private Image _connectionLineImage;
        [SerializeField] private Button _claimButton;
        [Space]
        [SerializeField] private StepWindow _stepWindow;
        [SerializeField] private StepWindowBig _stepWindowBig;
        [Space]
        [SerializeField] private CupsAmountSlider _cupsAmountSlider;

        public int GoalCupsAmount;
        public bool IsLast;

        public Image ConnectionLineImage => _connectionLineImage;
        public Button ClaimButton => _claimButton;
        public StepWindow StepWindow => _stepWindow;
        public StepWindowBig StepWindowBig => _stepWindowBig;
        public CupsAmountSlider CupsAmountSlider => _cupsAmountSlider;
    }
}

