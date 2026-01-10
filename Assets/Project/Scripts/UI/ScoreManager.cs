using TMPro;
using UnityEngine;

namespace TestBotRoom.UI
{
    public class ScoreManager : MonoBehaviour
    {
        
        [SerializeField] private TMP_Text worldScoreText;

        private int _coinCount;

        private void Start()
        {
            EventManager.Instance.AddListener(EventNameSaver.OnCoinColleted, IncreaseCoinCount);
            _coinCount = 0;
            UpdateUI(_coinCount);
        }

        private void IncreaseCoinCount()
        {
            _coinCount++;
            UpdateUI(_coinCount);
        }

        private void UpdateUI(int value)
        {
            _coinCount = value;
            worldScoreText.text = _coinCount.ToString();
        }

    }
}