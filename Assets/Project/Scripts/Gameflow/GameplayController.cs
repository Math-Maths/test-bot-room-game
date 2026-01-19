using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Experimental.GlobalIllumination;
using TestBotRoom.UI;
using UnityEngine.SocialPlatforms.Impl;
using TestBotRoom.Gameplay;

namespace TestBotRoom
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerInstance;
        [SerializeField] private CinemachineCamera _followPlayerCamera;
        [SerializeField] private Light _sunLight;
        [SerializeField] private Light _roomLight;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameplayUIControl _gameplayUIControl;
        [SerializeField] private CoinSpawner _coinSpawner;
        [SerializeField] private LaserSpawner _laserSpawner;

        private ScoreSystem _scoreSystem;

        private async void Start()
        {
            BindObjects();
            //Show some loading screen
            await InitialiazeObjects();
            PrepareGameplay();
            //Hide loading screen
            StartGamePlay();
        }

        private void BindObjects()
        {
            _scoreSystem = GetComponent<ScoreSystem>();
            _playerInstance = Instantiate(_playerInstance);
            _mainCamera = Instantiate(_mainCamera);
            _followPlayerCamera = Instantiate(_followPlayerCamera);
            _sunLight = Instantiate(_sunLight);
            _coinSpawner = Instantiate(_coinSpawner);
            _laserSpawner = Instantiate(_laserSpawner);
            _gameplayUIControl = Instantiate(_gameplayUIControl);
            _roomLight = Instantiate(_roomLight);
        }

        private async Awaitable InitialiazeObjects()
        {
            _playerInstance.OnInitiate();
            _coinSpawner.OnInitiate();
            _laserSpawner.OnInitiate();
            _scoreSystem.OnInitiate();
            _gameplayUIControl.OnInitiate();
        }

        private void PrepareGameplay()
        {
            _playerInstance.transform.position = Vector3.zero;
            _followPlayerCamera.LookAt = _playerInstance.transform;
        }

        private void StartGamePlay()
        {
            _playerInstance.StartGamePlay();
            _coinSpawner.CreateACoin();
            GameManager.Instance.StartGamePlay();
            _laserSpawner.StartLasers();
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, OnGameplayEnd);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, OnGameplayEnd);
        }

        private void OnGameplayEnd()
        {
            _gameplayUIControl.ShowEndScreen(_scoreSystem.CurrentScore);
        }
    }
}