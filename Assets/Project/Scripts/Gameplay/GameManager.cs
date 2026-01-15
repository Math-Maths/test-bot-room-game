using UnityEngine;

namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}

        private GameState _currentGameState;

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
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnPlayerDeath, PlayerDeath);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnPlayerDeath, PlayerDeath);
        }

        private void PlayerDeath()
        {
            EventManager.Instance.Invoke(EventNameSaver.OnGameOver);
            _currentGameState = GameState.GameOver;
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