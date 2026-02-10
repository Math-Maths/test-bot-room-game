using UnityEngine;

namespace TestBotRoom.Gameplay
{
    public class ScoreSystem : MonoBehaviour, IInitiation
    {
        private int _currentScore;
        private int _bestScore;

        public int CurrentScore => _currentScore;
        public int BestScore => _bestScore;

        public void OnInitiate()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnCoinColleted, OnCoinCollected);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, OnGameOver);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnCoinColleted, OnCoinCollected);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, OnGameOver);
        }

        public void PrepareScore(int currentBestScore)
        {
            _bestScore = currentBestScore;
        }

        private void OnCoinCollected()
        {
            _currentScore++;

            EventManager.Instance.Invoke(EventNameSaver.OnScoreChanged, _currentScore);
        }

        private void OnGameOver()
        {
            if (_currentScore > _bestScore)
            {
                _bestScore = _currentScore;
            }

            EventManager.Instance.Invoke(EventNameSaver.OnBestScoreChanged, _bestScore);
        }

        public void ResetScore()
        {
            _currentScore = 0;
            EventManager.Instance.Invoke(EventNameSaver.OnScoreChanged, _currentScore);
        }
    }
}
