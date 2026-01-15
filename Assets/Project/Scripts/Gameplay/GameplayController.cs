using UnityEngine;
using Unity.Cinemachine;

namespace TestBotRoom
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private CinemachineCamera followPlayerCamera;

        [Header("Time to Start Gameplay")]
        [SerializeField] private float delayTime;

        private PlayerController _playerInstance;

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnGameStarts, StarResetGameplay);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameStarts, StarResetGameplay);
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