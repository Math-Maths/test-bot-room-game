using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}

        private GameState _currentGameState;
        private DataManager _dataManager;
        private EventManager _eventManager;
        private GameStatus _gameStatus;

        public GameState CurrentGameState
        {
            get { return _currentGameState; }
            private set { _currentGameState = value; }
        }

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            if(Instance == null) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject); 

            _dataManager = GetComponent<DataManager>();
            _eventManager = GetComponent<EventManager>();
            _gameStatus = new GameStatus();

            BindEvents();
            LoadData();
        }

        private void BindEvents()
        {
            //_eventManager.AddListener(EventNameSaver.GoToGameplay, GoToGameplayScene);
        }

        private void OnDisable()
        {
            
        }

        public void StartGamePlay()
        {
            _currentGameState = GameState.Gameplay;
            EventManager.Instance.Invoke(EventNameSaver.OnGameStarts);
        }

        public void LoadData()
        {
            SaveData data = _dataManager.Load();
            _gameStatus = new GameStatus();

            if(data == null)
            {
                _gameStatus.PlayerName = "Player";
                _gameStatus.Coins = 0;
                _gameStatus.Gears = 0;
                _gameStatus.BestScore = 0;
                _gameStatus.unlockedAchivements = new List<string>();
                _gameStatus.unlockedCharacters = new List<string>();
                return;
            }

            _gameStatus.PlayerName = data.playerName;
            _gameStatus.Coins = data.coins;
            _gameStatus.Gears = data.gears;
            _gameStatus.BestScore = data.bestScore;
            _gameStatus.unlockedAchivements = data.unlockedAchivements;
            _gameStatus.unlockedCharacters = data.unlockedCharacters;
        }

        public void SaveData()
        {
            SaveData data = new SaveData
            {
                bestScore = _gameStatus.BestScore,
                coins = _gameStatus.Coins,
                gears = _gameStatus.Gears,
                playerName = _gameStatus.PlayerName,
                unlockedAchivements = _gameStatus.unlockedAchivements,
                unlockedCharacters = _gameStatus.unlockedCharacters
            };

            _dataManager.Save(data);
        }

        private void AddCoins(int coins)
        {
            _gameStatus.Coins += coins;
        }

        private void RegisterScore(int score)
        {
            if(score > _gameStatus.BestScore)
                _gameStatus.BestScore = score;
        }

        public void FinishRun(int coinsInThisRun)
        {
            AddCoins(coinsInThisRun);
            RegisterScore(coinsInThisRun);
            SaveData();
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
        Menu,
        Gameplay,
        Tutorial,
        GameOver
    }

    public class GameStatus
    {
        public string PlayerName { get; set; }
        public int Coins { get; set; }
        public int Gears { get; set; }
        public int BestScore { get; set; }
        public List<string> unlockedCharacters;
        public List<string> unlockedAchivements;
    }
}