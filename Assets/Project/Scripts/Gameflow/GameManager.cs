using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}

        private GameState _currentGameState;
        private DataManager _dataManager;
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
            _gameStatus = new GameStatus();

            LoadData();
        }

        private void OnEnable()
        {
            
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

            if(data == null)
            {
                _gameStatus = GameStatus.Default;
                return;
            }

            _gameStatus.PlayerName = data.playerName;
            _gameStatus.Coins = data.coins;
            _gameStatus.Gears = data.gears;
            _gameStatus.BestScore = data.bestScore;
        }

        public void SaveData()
        {
            SaveData data = new SaveData
            {
                bestScore = _gameStatus.BestScore,
                coins = _gameStatus.Coins,
                gears = _gameStatus.Gears,
                playerName = _gameStatus.PlayerName
            };

            _dataManager.Save(data);
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
        public static readonly GameStatus Default = new GameStatus
        {
            PlayerName = "Player",
            Coins = 0,
            Gears = 0,
            BestScore = 0
        };

        public string PlayerName { get; set; }
        public int Coins { get; set; }
        public int Gears { get; set; }
        public int BestScore { get; set; }
    }
}