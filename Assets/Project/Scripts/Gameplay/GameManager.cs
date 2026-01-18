using UnityEngine;
using UnityEngine.SceneManagement;

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

    public class SaveLoadSystem
    {
        
    }

    public enum GameState
    {
        Menu,
        Gameplay,
        Tutorial,
        GameOver
    }
}