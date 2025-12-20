using System.Collections.Generic;
using TowerDefence.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace TowerDefence.Game
{
    /// <summary>
    /// Main gameplay controller. Manages game session lifecycle.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [Header("Game Rules")]
        [SerializeField] private GameRulesManager _rulesManager;

        [Header("Player")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private CinemachineCamera _camera;

        [Header("Teams")]
        [SerializeField] private List<GameObject> _teamAMembers = new List<GameObject>();
        [SerializeField] private List<GameObject> _teamBMembers = new List<GameObject>();

        private IEventBus _eventBus;
        private IEventToken _gameOverToken;
        private GameObject _playerInstance;
        private bool _isGameActive;

        private void Awake()
        {
            _eventBus = Services.Get<IEventBus>();
        }

        private void Start()
        {
            InitializeGame();
        }

        private void OnDestroy()
        {
            CleanupGame();
        }

        private void InitializeGame()
        {
            // Subscribe to game over event
            _gameOverToken = _eventBus.Subscribe<GameOverEvent>(OnGameOver);

            // Spawn player
            SpawnPlayer();

            // Initialize game rules
            InitializeGameRules();

            // Start the game
            StartGame();
        }

        private void SpawnPlayer()
        {
            if (_playerPrefab == null)
            {
                Debug.LogError("Player prefab is not assigned!");
                return;
            }

            Vector3 spawnPosition = _playerSpawnPoint != null
                ? _playerSpawnPoint.position
                : Vector3.zero;

            _playerInstance = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
            _playerInstance.name = "Player";

            _camera.Follow = _playerInstance.transform;

            Debug.Log($"Player spawned at {spawnPosition}");
        }

        private void InitializeGameRules()
        {
            if (_rulesManager == null)
            {
                Debug.LogError("GameRulesManager is not assigned!");
                return;
            }

            // Collect all team members
            var allTeamMembers = new List<TeamMember>();

            foreach (var member in _teamAMembers)
            {
                if (member != null && member.TryGetComponent<TeamMember>(out var teamMember))
                {
                    allTeamMembers.Add(teamMember);
                }
            }

            foreach (var member in _teamBMembers)
            {
                if (member != null && member.TryGetComponent<TeamMember>(out var teamMember))
                {
                    allTeamMembers.Add(teamMember);
                }
            }

            // Add player to team if exists
            if (_playerInstance != null && _playerInstance.TryGetComponent<TeamMember>(out var playerTeam))
            {
                allTeamMembers.Add(playerTeam);
            }

            // Initialize game rules with team elimination strategy
            _rulesManager.InitializeGameRules(new TeamSwitchRule(), allTeamMembers);

            Debug.Log($"Game rules initialized with {allTeamMembers.Count} team members");
        }

        private void StartGame()
        {
            _isGameActive = true;
            Time.timeScale = 1f;

            Debug.Log("Game started!");
        }

        private void OnGameOver(GameOverEvent evt)
        {
            if (!_isGameActive)
            {
                return;
            }

            EndGame();
        }

        private void EndGame()
        {
            _isGameActive = false;
            Time.timeScale = 0f;

            Debug.Log("Game ended!");

            // Optionally show game over UI here
            // You can publish another event or directly show the game over screen
        }

        private void CleanupGame()
        {
            if (_eventBus != null && _gameOverToken != null)
            {
                _eventBus.Unsubscribe(_gameOverToken);
            }

            _isGameActive = false;
        }

        public bool IsGameActive() => _isGameActive;
    }
}