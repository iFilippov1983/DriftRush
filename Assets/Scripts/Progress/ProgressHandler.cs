using RaceManager.Root;
using UnityEngine;
using Zenject;

namespace RaceManager.Progress
{
    public class ProgressHandler : MonoBehaviour
    {
        private GameProgressScheme _gameProgressScheme;
        private RaceRewardsScheme _raceRewardsScheme;

        private PlayerProfile _playerProfile;
        private SaveManager _saveManager;

        [Inject]
        private void Construct(PlayerProfile playerProfile, SaveManager saveManager, GameProgressScheme gameProgressScheme, RaceRewardsScheme raceRewardsScheme)
        { 
            _playerProfile = playerProfile;
            _saveManager = saveManager;
            _gameProgressScheme = gameProgressScheme;
            _raceRewardsScheme = raceRewardsScheme;
        }


    }
}
