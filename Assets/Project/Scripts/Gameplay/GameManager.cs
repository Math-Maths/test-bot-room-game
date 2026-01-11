using Unity.Cinemachine;
using UnityEngine;

namespace TestBotRoom
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set;}

        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private CinemachineCamera followPlayerCamera;

        private bool _isGameRunning;
        private PlayerController _playerGO;

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
            EventManager.Instance.Invoke(EventNameSaver.OnGameOver);
            _isGameRunning = false;
        }

        public void PlayGame()
        {
            _isGameRunning = true;
            EventManager.Instance.Invoke(EventNameSaver.OnGameStarts);

            if(_playerGO == null)
                _playerGO = Instantiate(playerPrefab);
            else
                _playerGO.gameObject.SetActive(true);

            _playerGO.InitializePlayer();

            followPlayerCamera.LookAt = _playerGO.transform;
        }
        
    }
}