using UnityEngine;

namespace TestBotRoom
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
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnCoinColleted, ChangeCoinPosition);
        }

        public void CreateACoin()
        {
            float randomX = Random.Range(-mapWidth, mapLenght);
            float randomZ = Random.Range(-mapLenght, mapLenght);
            Vector3 finalPosition = new Vector3(randomX, coinHeight, randomZ);
            _currentCoin = Instantiate(coinPrefab, finalPosition, Quaternion.identity);
        }

        private void ChangeCoinPosition()
        {
            if(_currentCoin == null)
                return;

            float randomX = Random.Range(-mapWidth, mapLenght);
            float randomZ = Random.Range(-mapLenght, mapLenght);
            _currentCoin.transform.position = new Vector3(randomX, _currentCoin.transform.position.y, randomZ);
        }

    }
}