using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

namespace RaceManager.Tools
{
    public class PerformanceDisplayer
    {
        private PerformanceDisplayData _data;        

        private int _maxFpsToDisplay;
        private FpsCounter _fpsCounter;
        private string[] _stringArrayFrom00ToMax;

        public PerformanceDisplayer(PerformanceDisplayData fpsDisplayData)
        {
            _data = fpsDisplayData;
            SetData();
        }

        public void Display(float unscaledDeltaTime)
        {
            _fpsCounter.Calculte(unscaledDeltaTime);

            DisplayFpsOnLabel(_data.averageText, _fpsCounter.AverageFps);
            DisplayFpsOnLabel(_data.highestText, _fpsCounter.HighestFps);
            DisplayFpsOnLabel(_data.lowestText, _fpsCounter.LowestFps);

            _data.monoUsedText.text = Profiler.GetMonoUsedSizeLong().ToString();
            _data.monoHeapText.text = Profiler.GetMonoHeapSizeLong().ToString();
        }

        private void SetData()
        {
            _fpsCounter = new FpsCounter();

            _maxFpsToDisplay = _data.MaxFpsToDisplay;
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

        private void DisplayFpsOnLabel(TMP_Text label, int fps)
        {
            label.text = _stringArrayFrom00ToMax[Mathf.Clamp(fps, 0, _maxFpsToDisplay - 1)].ToString();
        }
    }

    public struct FpsColor
    {
        public Color color;
        public int fpsValue;
    }
}
