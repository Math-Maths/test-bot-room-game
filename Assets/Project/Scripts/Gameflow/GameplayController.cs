using UnityEngine;
using Unity.Cinemachine;
using TestBotRoom.UI;
using TestBotRoom.Gameplay;
using System.Threading.Tasks;

namespace TestBotRoom
{
    public class GameplayController : MonoBehaviour
    {
        [Header("Prefabs References")]
        [SerializeField] private PlayerController _playerInstance;
        [SerializeField] private CinemachineCamera _followPlayerCamera;
        [SerializeField] private GameObject _lightHolder;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameplayUIControl _gameplayUIControl;
        [SerializeField] private CoinSpawner _coinSpawner;
        [SerializeField] private LaserSpawner _laserSpawner;
        [SerializeField] private LoadingScreenControl _loadingScreen;

        [Header("Scene Settings")]
        [SerializeField] private float gameDelayStart;

        private ScoreSystem _scoreSystem;
        private GameStatus _gameplayStatus;

        private bool _firstContinue;

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, OnGameplayEnd);
            EventManager.Instance.AddListener(EventNameSaver.OnGameReset, ResetGamePlay);
            EventManager.Instance.AddListener(EventNameSaver.GoToMenu, GoToMainMenu);
            EventManager.Instance.AddListener(EventNameSaver.OnContinueGameplay, ContinueGameplay);

            //Provisional
            //EventManager.Instance.AddListener(EventNameSaver.ProvisionalPlay, PlayProvisional);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, OnGameplayEnd);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameReset, ResetGamePlay);
            EventManager.Instance.RemoveListener(EventNameSaver.GoToMenu, GoToMainMenu);
            EventManager.Instance.RemoveListener(EventNameSaver.OnContinueGameplay, ContinueGameplay);

            //Provisional
            //EventManager.Instance.RemoveListener(EventNameSaver.ProvisionalPlay, PlayProvisional);
        }

        private async void Start()
        {
            GetStatus();
            BindObjects();
            //Show some loading screen
            _loadingScreen.ShowLoadScreen();
            await InitialiazeObjects();
            PrepareGameplay();
            //Hide loading screen
            _loadingScreen.HideLoadingScreen();
            await _gameplayUIControl.ShowCountdown(gameDelayStart);
            StartGamePlay();
        }

        private void BindObjects()
        {
            _scoreSystem = GetComponent<ScoreSystem>();
            _playerInstance = Instantiate(_playerInstance);
            _mainCamera = Instantiate(_mainCamera);
            _followPlayerCamera = Instantiate(_followPlayerCamera);
            _lightHolder = Instantiate(_lightHolder);
            _coinSpawner = Instantiate(_coinSpawner);
            _laserSpawner = Instantiate(_laserSpawner);
            _gameplayUIControl = Instantiate(_gameplayUIControl);
            _loadingScreen = Instantiate(_loadingScreen);
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
            //TODO: implement new data format
            //_scoreSystem.PrepareScore(_gameplayStatus.BestScore);
            _playerInstance.transform.position = Vector3.zero;
            _followPlayerCamera.LookAt = _playerInstance.transform;
        }

        private void StartGamePlay()
        {
            _playerInstance.StartGamePlay();
            _coinSpawner.CreateACoin();
            GameManager.Instance.StartGamePlay();
            _laserSpawner.StartLasers();
            _firstContinue = true;
        }

        private void ResetGamePlay()
        {
            _ = ResetGamePlayAsync();
        }

        private async Task ResetGamePlayAsync()
        {
            await EndRunAndReport();

            _playerInstance.ResetPosition();
            _followPlayerCamera.LookAt = _playerInstance.transform;
            _scoreSystem.ResetScore();

            await _gameplayUIControl.ShowCountdown(gameDelayStart);

            GameManager.Instance.StartGamePlay();
            _playerInstance.StartGamePlay();
            _laserSpawner.StartLasers();
            _coinSpawner.CreateACoin();
            _firstContinue = true;
            _gameplayUIControl.ResetContinueButton();
        }

        private void ContinueGameplay()
        {
            _ = ContinueGameplayAsync();
        }

        private async Task ContinueGameplayAsync()
        {
            if(!_firstContinue)
                return;

            
            //TODO: await play AD
            _playerInstance.ResetPosition();
            _followPlayerCamera.LookAt = _playerInstance.transform;

            await _gameplayUIControl.ShowCountdown(gameDelayStart);
            GameManager.Instance.StartGamePlay();
            _playerInstance.StartGamePlay();
            _laserSpawner.StartLasers(true);
            _coinSpawner.CreateACoin();
            _firstContinue = false;
        }

        private void OnGameplayEnd()
        {
            _gameplayUIControl.ShowEndScreen(_scoreSystem.CurrentScore, _firstContinue);
        }

        private void GoToMainMenu()
        {
            _ = GoToMainMenuAsync();
        }

        private async Task GoToMainMenuAsync()
        {
            _gameplayUIControl.ResetContinueButton();
            _loadingScreen.ShowLoadScreen();
            await EndRunAndReport();

            GameManager.Instance.ChangeScene("Menu_Scene");
        }

        private async Task EndRunAndReport()
        {
            int finalscore = _scoreSystem.CurrentScore;

            await GameManager.Instance.FinishRun(finalscore);
        }

        private void GetStatus()
        {
            _gameplayStatus = GameManager.Instance.GetPlayerData();
        }

        //Provisional Method
        //private void PlayProvisional()
        //{
        //    _ = ProvisionalPlayGame();
        //}

        //private async Task ProvisionalPlayGame()
        //{
        //    await _gameplayUIControl.ShowCountdown(gameDelayStart);
        //    StartGamePlay();
        //}
    }
}