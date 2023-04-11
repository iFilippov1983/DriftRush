using UnityEngine;

namespace RaceManager.Tools
{
    public class FpsCounter
    {
        
        private int[] _fpsBuffer;
        private int _fpsBufferIndex;

        public int _frameRange = 60;
        public int AverageFps { get; private set; }
        public int HighestFps { get; private set; }
        public int LowestFps { get; private set; }

        public void Calculte(float unscaledDeltaTime)
        {
            if (_fpsBuffer == null || _frameRange != _fpsBuffer.Length)
            {
                InitializeBuffer();
            }

            UpdateBuffer(unscaledDeltaTime);
            CalculateFps();
        }

        private void InitializeBuffer()
        {
            if (_frameRange <= 0) _frameRange = 1;

            _fpsBuffer = new int[_frameRange];
            _fpsBufferIndex = 0;
        }

        private void UpdateBuffer(float unscaledDeltaTime)
        {
            _fpsBuffer[_fpsBufferIndex++] = (int)(1f / unscaledDeltaTime);

            if (_fpsBufferIndex >= _frameRange) _fpsBufferIndex = 0;

        }

        private void CalculateFps()
        {
            int sum = 0;
            int lowest = int.MaxValue;
            int highest = 0;

            for (int i = 0; i < _frameRange; i++)
            {
                int fps = _fpsBuffer[i];
                sum += fps;
                if (fps > highest) highest = fps;
                if (fps < lowest) lowest = fps;
            }
            HighestFps = highest;
            LowestFps = lowest;
            AverageFps = sum / _frameRange;
        }
    }
}


