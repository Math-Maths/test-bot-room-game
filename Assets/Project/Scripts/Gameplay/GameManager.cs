using UnityEngine;

namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}

        public enum GameState
        {
            Menu,
            Playing,
            GameOver
        }

        private bool _isGameRunning;

        public bool IsGameRunning
        {
            get { return _isGameRunning;}
        }

        private void Awake()
        {
            if(Instance == null) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            EventManager.Instance.AddListener(EventNameSaver.OnPlayerDeath, PlayerDeath);
        }

        private void PlayerDeath()
        {
            //Debug.Log("Game Over");
            EventManager.Instance.Invoke(EventNameSaver.OnGameOver);
            _isGameRunning = false;
        }

        public void StartGamePlay()
        {
            _isGameRunning = true;
            EventManager.Instance.Invoke(EventNameSaver.OnGameStarts);
        }

    }
}