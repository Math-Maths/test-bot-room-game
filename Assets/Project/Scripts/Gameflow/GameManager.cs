using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Services.Authentication;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.UnityConsent;
using System;


namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}

        private GameState _currentGameState;
        private DataManager _dataManager;
        private EventManager _eventManager;
        private GameStatus _gameStatus;
        private CloudDataManager _cloudDataManager;
        private LoginManager _loginManager;

        public GameState CurrentGameState
        {
            get { return _currentGameState; }
            private set { _currentGameState = value; }
        }

        private async void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            if(Instance == null)
            { 
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            GetAndSetReferences();

            await PlayerValidation();
        }

        private async Task PlayerValidation()
        {
            try 
            {
                await InitializeServices();

                bool isAuthenticated = await CheckUserStatusAndProceed();

                if (isAuthenticated)
                {
                    await LoadData();
                }
                else 
                {
                    return;
                }
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
            _eventManager = GetComponent<EventManager>();
            _cloudDataManager = GetComponent<CloudDataManager>();
            _loginManager = GetComponent<LoginManager>();
            _loginManager.Initialize(this);
            _gameStatus = new GameStatus();
        }

        private async Task InitializeServices()
        {
            if(UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            EndUserConsent.SetConsentState(new ConsentState {
                AnalyticsIntent = ConsentStatus.Granted,
                AdsIntent = ConsentStatus.Denied
            });
        }

        public async Task<bool> CheckUserStatusAndProceed()
        {
            if (AuthenticationService.Instance.SessionTokenExists)
            {
                try 
                {
                    string lastMethod = PlayerPrefs.GetString(LoginManager.LOGIN_KEY, LoginManager.METHOD_ANONYMOUS);

                    if (lastMethod == LoginManager.METHOD_GOOGLE)
                    {
                        await _loginManager.SignInOrLinkWithGooglePlayGames();
                    }
                    else
                    {
                        await _loginManager.StartAnonymousSignIn();
                    }

                    Debug.Log("User already authenticated.");

                    return true;
                }
                catch 
                {
                    Debug.LogWarning("Existing session token is invalid. Starting new authentication flow.");

                    return false;
                }
            }

            EventManager.Instance.Invoke(EventNameSaver.ShowSignInOptions);
            return false;
        }

        public void StartGamePlay()
        {
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
            SaveData localData = _dataManager.Load();
            if(localData != null) 
            {
                ApplyDataToGame(localData);
                EventManager.Instance.Invoke(EventNameSaver.ShowLobby);
            }
            else 
            {
                _gameStatus = new GameStatus { PlayerName = "Offline Player", BestScore = 0 };
                _dataManager.Save(new SaveData { playerName = "Offline Player" });
            }
        }

        private void ApplyDataToGame(SaveData data)
        {
            _gameStatus.PlayerName = data.playerName;
            _gameStatus.BestScore = data.bestScore;
            _gameStatus.unlockedAchivements = data.unlockedAchivements;
        }

        private async Task CreateDefaultData()
        {
            _gameStatus = new GameStatus {
                PlayerName = "New Player",
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
            if(score > _gameStatus.BestScore)
                _gameStatus.BestScore = score;
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

            Debug.Log($"Player name updated to: {_gameStatus.PlayerName}");
        }

        public async Task FinishRun(int coinsInThisRun)
        {
            RegisterScore(coinsInThisRun);
            await SaveData();
        }

        public void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public GameStatus GetPlayerData()
        {
            return _gameStatus;
        }
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
}