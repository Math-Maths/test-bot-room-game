using TMPro;
using UnityEngine;

namespace TestBotRoom.UI
{
    public class ScoreManager : MonoBehaviour
    {
        
        [SerializeField] private TMP_Text worldScoreText;
        [SerializeField] private TMP_Text bestScoreText;

        private int _coinCount;
        private int _bestScore;

        private void Start()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnCoinColleted, IncreaseCoinCount);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, ScoreReset);
            _coinCount = 0;
            UpdateUI(_coinCount);
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnCoinColleted, IncreaseCoinCount);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, ScoreReset);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNameSaver.OnCoinColleted, IncreaseCoinCount);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, ScoreReset);
        }

        private void IncreaseCoinCount()
        {
            _coinCount++;
            UpdateUI(_coinCount);
        }

        private void UpdateUI(int value)
        {
            _coinCount = value;
            worldScoreText.text = _coinCount.ToString("000");
        }

        private void ScoreReset()
        {
            if(_coinCount > _bestScore)
            {
                _bestScore = _coinCount;
                bestScoreText.text = _bestScore.ToString("000");
            }

            UpdateUI(0);
        }

    }
}