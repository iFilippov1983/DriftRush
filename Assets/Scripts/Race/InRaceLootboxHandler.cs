using RaceManager.Progress;
using UnityEngine;

namespace RaceManager.Race
{
    public class InRaceLootboxHandler
    {
        private bool _needUpdate;

        private Lootbox _lootboxToHandle;

        public InRaceLootboxHandler(Profiler profiler)
        {
            _needUpdate = profiler.TryGetLootboxWhithActiveTimer(out _lootboxToHandle);
        }

        /// <summary>
        /// Handles open time of lootbox whith active timer. Need to be called in FixedUpdate
        /// </summary>
        public void Handle()
        {
            if (!_needUpdate)
                return;

            _lootboxToHandle.TimeToOpenLeft -= Time.deltaTime;
        }
    }
}