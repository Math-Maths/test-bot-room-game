using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.UnityConsent;
using System;

namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        private const string DefaultPlayerName = "New Player";

        public static GameManager Instance { get; private set; }

        private AppFlowState _currentAppFlowState;
        private GameState _currentGameState;
        private DataManager _dataManager;
        private GameStatus _gameStatus;
        private CloudDataManager _cloudDataManager;
        private LoginManager _loginManager;

        public AppFlowState CurrentAppFlowState => _currentAppFlowState;

        public GameState CurrentGameState
        {
            get { return _currentGameState; }
            private set { _currentGameState = value; }
        }

        private async void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            GetAndSetReferences();

            await BootstrapAsync();
        }

        private async Task BootstrapAsync()
        {
            TransitionToAppFlow(AppFlowState.Booting);

            try
            {
                await InitializeServices();

                AuthResult authResult = await CheckUserStatusAndProceed();
                await HandleAuthenticationResult(authResult);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Offline mode activated: " + ex.Message);
                LoadOfflineMode();
            }
        }

        private void GetAndSetReferences()
        {
            _dataManager = GetComponent<DataManager>();
            _cloudDataManager = GetComponent<CloudDataManager>();
            _loginManager = GetComponent<LoginManager>();
            _loginManager.Initialize(this);
            _gameStatus = new GameStatus();
        }

        private async Task InitializeServices()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            EndUserConsent.SetConsentState(new ConsentState
            {
                AnalyticsIntent = ConsentStatus.Granted,
                AdsIntent = ConsentStatus.Denied
            });
        }

        public async Task<AuthResult> CheckUserStatusAndProceed()
        {
            if (AuthenticationService.Instance.SessionTokenExists)
            {
                try
                {
                    AuthResult authResult = await _loginManager.SignInWithSavedMethodAsync();

                    if (authResult.Success)
                    {
                        Debug.Log("User already authenticated.");
                    }

                    return authResult;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Existing session token is invalid. Starting new authentication flow. " + ex.Message);
                }
            }

            EnterLoginSelection();
            return AuthResult.NoStoredSession();
        }

        public async Task HandleAuthenticationResult(AuthResult authResult)
        {
            if (!authResult.Success)
            {
                if (authResult.ShouldShowLoginOptions)
                {
                    EnterLoginSelection();
                }

                return;
            }

            TransitionToAppFlow(AppFlowState.LoadingProfile);
            await LoadData();
            RouteAuthenticatedPlayer();
        }

        public void StartGamePlay()
        {
            TransitionToAppFlow(AppFlowState.Gameplay);
            _currentGameState = GameState.Gameplay;
            EventManager.Instance.Invoke(EventNameSaver.OnGameStarts);
        }

        public async Task LoadData()
        {
            string cloudJson = await _cloudDataManager.GetPlayerDataFromCloud();

            if (!string.IsNullOrEmpty(cloudJson))
            {
                Debug.Log("Dados carregados da nuvem.");
                SaveData data = JsonUtility.FromJson<SaveData>(cloudJson);
                ApplyDataToGame(data);
                _dataManager.Save(data);
            }
            else
            {
                SaveData localData = _dataManager.Load();

                if (localData != null)
                {
                    Debug.Log("Load data from local cache.");
                    ApplyDataToGame(localData);
                }
                else
                {
                    Debug.Log("First time player.");
                    await CreateDefaultData();
                }
            }
        }

        private void LoadOfflineMode()
        {
            TransitionToAppFlow(AppFlowState.Offline);

            SaveData localData = _dataManager.Load();
            if (localData != null)
            {
                ApplyDataToGame(localData);
                EnterLobby();
            }
            else
            {
                _gameStatus = new GameStatus
                {
                    PlayerName = "Offline Player",
                    BestScore = 0,
                    unlockedAchivements = new List<string>()
                };
                _dataManager.Save(new SaveData { playerName = "Offline Player" });
                EnterProfileSetup();
            }
        }

        private void ApplyDataToGame(SaveData data)
        {
            _gameStatus.PlayerName = data.playerName;
            _gameStatus.BestScore = data.bestScore;
            _gameStatus.unlockedAchivements = data.unlockedAchivements ?? new List<string>();
        }

        private async Task CreateDefaultData()
        {
            _gameStatus = new GameStatus
            {
                PlayerName = DefaultPlayerName,
                BestScore = 0,
                unlockedAchivements = new List<string>()
            };

            await SaveData();
        }

        public async Task SaveData()
        {
            SaveData data = new SaveData
            {
                bestScore = _gameStatus.BestScore,
                playerName = _gameStatus.PlayerName,
                unlockedAchivements = _gameStatus.unlockedAchivements
            };

            _dataManager.Save(data);

            if (UnityServices.State == ServicesInitializationState.Initialized &&
                AuthenticationService.Instance.IsSignedIn)
            {
                try
                {
                    await _cloudDataManager.SavePlayerDataToCloud(data);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("It was not possible to synchronize with the cloud now: " + e.Message);
                }
            }
        }

        private void RegisterScore(int score)
        {
            if (score > _gameStatus.BestScore)
            {
                _gameStatus.BestScore = score;
            }
        }

        public async Task SavePlayerName(string newName)
        {
            if (_gameStatus == null)
            {
                Debug.LogWarning("GameStatus not initialized. Creating new instance.");
                _gameStatus = new GameStatus();
            }

            _gameStatus.PlayerName = newName.Trim();
            await SaveData();
            EnterLobby();

            Debug.Log($"Player name updated to: {_gameStatus.PlayerName}");
        }

        public async Task FinishRun(int coinsInThisRun)
        {
            RegisterScore(coinsInThisRun);
            await SaveData();
        }

        public async Task StartAnonymousSignInAsync()
        {
            AuthResult authResult = await _loginManager.StartAnonymousSignIn();
            await HandleAuthenticationResult(authResult);
        }

        public async Task StartGooglePlayGamesSignInAsync()
        {
            AuthResult authResult = await _loginManager.StartSignInWithGooglePlayGames();
            await HandleAuthenticationResult(authResult);
        }

        public void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public GameStatus GetPlayerData()
        {
            return _gameStatus;
        }

        private void RouteAuthenticatedPlayer()
        {
            EventManager.Instance.Invoke(EventNameSaver.HideSignInOptions);

            if (HasCompletedPlayerProfile())
            {
                EnterLobby();
                return;
            }

            EnterProfileSetup();
        }

        private bool HasCompletedPlayerProfile()
        {
            if (_gameStatus == null)
            {
                return false;
            }

            string playerName = _gameStatus.PlayerName?.Trim();

            return !string.IsNullOrWhiteSpace(playerName) &&
                   !string.Equals(playerName, DefaultPlayerName, StringComparison.Ordinal) &&
                   playerName.Length is >= 4 and <= 16;
        }

        private void EnterLoginSelection()
        {
            TransitionToAppFlow(AppFlowState.NeedsLoginChoice);
            _currentGameState = GameState.StartMenu;
            EventManager.Instance.Invoke(EventNameSaver.ShowSignInOptions);
        }

        private void EnterProfileSetup()
        {
            TransitionToAppFlow(AppFlowState.NeedsProfileSetup);
            _currentGameState = GameState.StartMenu;
            EventManager.Instance.Invoke(EventNameSaver.HideSignInOptions);
        }

        private void EnterLobby()
        {
            TransitionToAppFlow(AppFlowState.Lobby);
            _currentGameState = GameState.Lobby;
            EventManager.Instance.Invoke(EventNameSaver.HideSignInOptions);
            EventManager.Instance.Invoke(EventNameSaver.ShowLobby);
        }

        private void TransitionToAppFlow(AppFlowState nextState)
        {
            _currentAppFlowState = nextState;
        }
    }

    public enum AppFlowState
    {
        Booting,
        NeedsLoginChoice,
        LoadingProfile,
        NeedsProfileSetup,
        Lobby,
        Gameplay,
        Offline
    }

    public enum GameState
    {
        StartMenu,
        Lobby,
        Gameplay,
        Tutorial,
        GameOver
    }

    public class GameStatus
    {
        public string PlayerName { get; set; }
        public int BestScore { get; set; }
        public List<string> unlockedAchivements;
    }

    public readonly struct AuthResult
    {
        public AuthResult(bool success, string provider, string errorMessage = null, bool shouldShowLoginOptions = true)
        {
            Success = success;
            Provider = provider;
            ErrorMessage = errorMessage;
            ShouldShowLoginOptions = shouldShowLoginOptions;
        }

        public bool Success { get; }
        public string Provider { get; }
        public string ErrorMessage { get; }
        public bool ShouldShowLoginOptions { get; }

        public static AuthResult Succeeded(string provider)
        {
            return new AuthResult(true, provider, shouldShowLoginOptions: false);
        }

        public static AuthResult Failed(string provider, string errorMessage, bool shouldShowLoginOptions = true)
        {
            return new AuthResult(false, provider, errorMessage, shouldShowLoginOptions);
        }

        public static AuthResult NoStoredSession()
        {
            return new AuthResult(false, null, shouldShowLoginOptions: false);
        }
    }
}
