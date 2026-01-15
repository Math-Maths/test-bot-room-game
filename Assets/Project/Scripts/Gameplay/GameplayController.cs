using UnityEngine;
using Unity.Cinemachine;

namespace TestBotRoom
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private CinemachineCamera followPlayerCamera;

        private PlayerController _playerInstance;

        private void Start()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnGameStarts, StarResetGameplay);
        }

        public void StarResetGameplay()
        {
            if (_playerInstance == null)
            {
                _playerInstance = Instantiate(playerPrefab);
                _playerInstance.InitializePlayer();
            }
            else
            {
                _playerInstance.InitializePlayer();
                _playerInstance.gameObject.SetActive(true);
            }

            followPlayerCamera.LookAt = _playerInstance.transform;
        }
    }
}