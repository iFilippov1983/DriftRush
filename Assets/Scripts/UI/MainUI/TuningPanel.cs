using RaceManager.Cars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class TuningPanel : MonoBehaviour
    {
        [SerializeField] private Button _tuneStatsButton;
        [SerializeField] private Button _tuneWheelsViewButton;
        [SerializeField] private Button _tuneCarViewButton;
        [Space]
        [SerializeField] private Slider _speedSlider;
        [SerializeField] private Slider _mobilitySlider;
        [SerializeField] private Slider _durabilitySlider;
        [SerializeField] private Slider _accelerationSlider;
        [Space]
        [SerializeField] private TMP_Text _carNameText;
        [SerializeField] private TMP_Text _carStatsProgressText;

        public Button TuneStatsButton => _tuneStatsButton;
        public Button TuneWheelsViewButton => _tuneWheelsViewButton;
        public Button TuneCarViewButton => _tuneCarViewButton;
        public Slider SpeedSlider => _speedSlider;
        public Slider MobilitySlider => _mobilitySlider;
        public Slider DurabilitySlider => _durabilitySlider;
        public Slider AccelerationSlider => _accelerationSlider;
        public TMP_Text CarNameText => _carNameText;
        public TMP_Text CarStatsProgressText => _carStatsProgressText;

        public void SetBorderValueToSlider(CarCharacteristics characteristics, int minValue, int maxValue)
        {
            switch (characteristics)
            {
                case CarCharacteristics.Speed:
                    SpeedSlider.minValue = minValue;
                    SpeedSlider.maxValue = maxValue;
                    break;
                case CarCharacteristics.Mobility:
                    MobilitySlider.minValue = minValue;
                    MobilitySlider.maxValue = maxValue;
                    break;
                case CarCharacteristics.Durability:
                    DurabilitySlider.minValue = minValue;
                    DurabilitySlider.maxValue = maxValue;
                    break;
                case CarCharacteristics.Acceleration:
                    AccelerationSlider.minValue = minValue;
                    AccelerationSlider.maxValue = maxValue;
                    break;
            }
        }

        public void SetValueToSlider(CarCharacteristics characteristics, int value)
        {
            switch (characteristics)
            {
                case CarCharacteristics.Speed:
                    SpeedSlider.value = value;
                    break;
                case CarCharacteristics.Mobility:
                    MobilitySlider.value = value;
                    break;
                case CarCharacteristics.Durability:
                    DurabilitySlider.value = value;
                    break;
                case CarCharacteristics.Acceleration:
                    AccelerationSlider.value = value;
                    break;
            }
        }

        public void SetInitialSlidersValues(int speed, int mobility, int durability, int acceleration)
        { 
            SpeedSlider.value = speed;
            MobilitySlider.value = mobility;
            DurabilitySlider.value = durability;
            AccelerationSlider.value = acceleration;
        }
    }
}