using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Experimental.GlobalIllumination;
using TestBotRoom.UI;
using UnityEngine.SocialPlatforms.Impl;
using TestBotRoom.Gameplay;
using System.Threading.Tasks;

namespace TestBotRoom
{
    public class GameplayController : MonoBehaviour
    {
        [Header("Prefabs References")]
        [SerializeField] private PlayerController _playerInstance;
        [SerializeField] private CinemachineCamera _followPlayerCamera;
        // [SerializeField] private Light _sunLight;
        // [SerializeField] private Light _roomLight;
        [SerializeField] private GameObject _lightHolder;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameplayUIControl _gameplayUIControl;
        [SerializeField] private CoinSpawner _coinSpawner;
        [SerializeField] private LaserSpawner _laserSpawner;

        [Header("Scene Settings")]
        [SerializeField] private float gameDelayStart;

        private ScoreSystem _scoreSystem;

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, OnGameplayEnd);
            EventManager.Instance.AddListener(EventNameSaver.OnGameReset, ResetGamePlay);

            //Provisional
            EventManager.Instance.AddListener(EventNameSaver.ProvisionalPlay, PlayProvisional);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, OnGameplayEnd);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameReset, ResetGamePlay);

            //Provisional
            EventManager.Instance.RemoveListener(EventNameSaver.ProvisionalPlay, PlayProvisional);
        }

        private async void Start()
        {
            BindObjects();
            //Show some loading screen
            await InitialiazeObjects();
            PrepareGameplay();
            //await _gameplayUIControl.ShowCountdown(gameDelayStart);
            //Hide loading screen
            //StartGamePlay();
        }

        private void BindObjects()
        {
            _scoreSystem = GetComponent<ScoreSystem>();
            _playerInstance = Instantiate(_playerInstance);
            _mainCamera = Instantiate(_mainCamera);
            _followPlayerCamera = Instantiate(_followPlayerCamera);
            _lightHolder = Instantiate(_lightHolder);
            //_sunLight = Instantiate(_sunLight);
            _coinSpawner = Instantiate(_coinSpawner);
            _laserSpawner = Instantiate(_laserSpawner);
            _gameplayUIControl = Instantiate(_gameplayUIControl);
            //_roomLight = Instantiate(_roomLight);
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
            Debug.Log("Playing");
            _playerInstance.StartGamePlay();
            _coinSpawner.CreateACoin();
            GameManager.Instance.StartGamePlay();
            _laserSpawner.StartLasers();
        }

        private void ResetGamePlay()
        {
            _ = ResetGamePlayAsync();
        }

        private async Task ResetGamePlayAsync()
        {
            _playerInstance.ResetPosition();
            _followPlayerCamera.LookAt = _playerInstance.transform;
            _scoreSystem.ResetScore();

            await _gameplayUIControl.ShowCountdown(gameDelayStart);

            GameManager.Instance.StartGamePlay();
            _playerInstance.StartGamePlay();
            _laserSpawner.StartLasers();
            _coinSpawner.CreateACoin();
        }

        private void OnGameplayEnd()
        {
            _gameplayUIControl.ShowEndScreen(_scoreSystem.CurrentScore);
        }

        //Provisional Method
        private void PlayProvisional()
        {
            _ = ProvisionalPlayGame();
        }

        private async Task ProvisionalPlayGame()
        {
            await _gameplayUIControl.ShowCountdown(gameDelayStart);
            StartGamePlay();
        }
    }
}