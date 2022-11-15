using RaceManager.Root;
using UnityEngine;
using Zenject;

namespace RaceManager.Progress
{
    public class ProgressHandler : MonoBehaviour
    {
        [SerializeField] private GameProgressScheme _gameProgressScheme;
        [SerializeField] private RaceRewardsScheme _raceRewardsScheme;

        private PlayerProfile _playerProfile;
        private SaveManager _saveManager;

        [Inject]
        private void Construct(PlayerProfile playerProfile, SaveManager saveManager)
        { 
            _playerProfile = playerProfile;
            _saveManager = saveManager;
        }


    }
}
