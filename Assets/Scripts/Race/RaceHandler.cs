using Cinemachine;
using RaceManager.Root;
using RaceManager.Cars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RaceManager.Race
{
    internal class RaceHandler : Singleton<RaceHandler>
    {
        private Driver _playerDriver;

        public void Setup(Driver playerDriver)
        {
            _playerDriver = playerDriver;
        }
    }
}
