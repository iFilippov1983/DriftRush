using TMPro;
using UnityEngine;

namespace RaceManager.Tools
{
    public class FpsDisplayer
    {
        private FpsDisplayData _fpsDisplayData;        

        private int _maxFpsToDisplay;
        private FpsColor[] _fpsColors;
        private FpsCounter _fpsCounter;
        private string[] _stringArrayFrom00ToMax;

        public FpsDisplayer(FpsDisplayData fpsDisplayData)
        {
            _fpsDisplayData = fpsDisplayData;
            SetData();
        }

        public void Display(float unscaledDeltaTime)
        {
            _fpsCounter.Calculte(unscaledDeltaTime);

            DisplayOnLabel(_fpsDisplayData.averageText, _fpsCounter.AverageFps);

            DisplayOnLabel(_fpsDisplayData.highestText, _fpsCounter.HighestFps);

            DisplayOnLabel(_fpsDisplayData.lowestText, _fpsCounter.LowestFps);
        }

        private void SetData()
        {
            _fpsCounter = new FpsCounter();

            _fpsColors = SetColors(_fpsDisplayData.Colors);

            _maxFpsToDisplay = _fpsDisplayData.MaxFpsToDisplay;
            _stringArrayFrom00ToMax = InitializeStringArray();
        }

        private string[] InitializeStringArray()
        {
            string[] array = new string[_maxFpsToDisplay];
            int strToAdd = 0;

            for (int i = 0; i < _maxFpsToDisplay; i++)
            {
                array[i] = strToAdd.ToString();
                strToAdd++;
            }
            return array;
        }

        private FpsColor[] SetColors(Color[] colorsArray)
        {
            FpsColor[] array = new FpsColor[colorsArray.Length];

            for (int i = 0; i < array.Length; i++)
            {
                array[i].color = colorsArray[i];
                if (i == 0) array[i].fpsValue = _maxFpsToDisplay / colorsArray.Length;
                else array[i].fpsValue = (_maxFpsToDisplay / colorsArray.Length) * (i + 1);
            }

            return array;
        }

        private void DisplayOnLabel(TMP_Text label, int fps)
        {
            label.text = _stringArrayFrom00ToMax[Mathf.Clamp(fps, 0, _maxFpsToDisplay - 1)].ToString();
            for (int i = 0; i < _fpsColors.Length; i++)
            {
                if (fps <= _fpsColors[i].fpsValue)
                {
                    label.color = _fpsColors[i].color;
                    break;
                }
            }
        }
    }

    public struct FpsColor
    {
        public Color color;
        public int fpsValue;
    }
}
