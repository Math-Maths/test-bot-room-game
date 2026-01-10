using UnityEngine;

namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}
        
        private bool _isGameRunning;

        public bool IsGameRunning
        {
            get { return _isGameRunning;}
        }

        private void Awake()
        {
            if(Instance == null) Instance = this;
            else Destroy(gameObject);

            EventManager.Instance.AddListener(EventNameSaver.OnPlayerDeath, PlayerDeath);
        }

        private void PlayerDeath()
        {
            Debug.Log("Game Over");
            _isGameRunning = false;
        }

    }
}