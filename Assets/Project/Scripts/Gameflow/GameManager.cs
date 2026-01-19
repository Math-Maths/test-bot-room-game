using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}

        private GameState _currentGameState;
        private DataManager _dataManager;

        public GameState CurrentGameState
        {
            get { return _currentGameState; }
            private set { _currentGameState = value; }
        }

        private void Awake()
        {
            if(Instance == null) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject); 

            _dataManager = GetComponent<DataManager>();
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

    }

    public enum GameState
    {
        Menu,
        Gameplay,
        Tutorial,
        GameOver
    }
}