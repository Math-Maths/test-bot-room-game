using UnityEngine;

namespace TestBotRoom.Gameplay
{
    public class CoinSpawner : MonoBehaviour, IInitiation
    {
        [SerializeField] private CoinBehavior coinPrefab;
        [SerializeField] private float mapWidth;
        [SerializeField] private float mapLenght;
        [SerializeField] private float coinHeight;

        private CoinBehavior _currentCoin;

        public void OnInitiate()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnCoinColleted, ChangeCoinPosition);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, DisableCoin);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnCoinColleted, ChangeCoinPosition);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, DisableCoin);
        }

        public void CreateACoin()
        {
            if(_currentCoin != null)
            {
                ChangeCoinPosition();
                return;
            }

            float randomX = Random.Range(-mapWidth, mapLenght);
            float randomZ = Random.Range(-mapLenght, mapLenght);
            Vector3 finalPosition = new Vector3(randomX, coinHeight, randomZ);
            _currentCoin = Instantiate(coinPrefab, finalPosition, Quaternion.identity);
            _currentCoin.SpawnAnimation();
        }

        private void ChangeCoinPosition()
        {
            if(_currentCoin == null)
                return;

            _currentCoin.gameObject.SetActive(true);

            float randomX = Random.Range(-mapWidth, mapLenght);
            float randomZ = Random.Range(-mapLenght, mapLenght);
            _currentCoin.transform.position = new Vector3(randomX, _currentCoin.transform.position.y, randomZ);
            _currentCoin.SpawnAnimation();
        }

        private void DisableCoin()
        {
            if(_currentCoin != null)
                _currentCoin.gameObject.SetActive(false);
        }

    }
}