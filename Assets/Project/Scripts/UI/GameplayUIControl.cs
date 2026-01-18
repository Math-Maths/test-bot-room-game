using TMPro;
using UnityEngine;

namespace TestBotRoom.UI
{
    public class GameplayUIControl : MonoBehaviour, IInitiation
    {
        [SerializeField] private TMP_Text worldScoreText;
        [SerializeField] private TMP_Text bestScoreText;

        public void OnInitiate()
        {
            EventManager.Instance.AddListener<int>(EventNameSaver.OnScoreChanged, UpdateScore);
            EventManager.Instance.AddListener<int>(EventNameSaver.OnBestScoreChanged, UpdateBestScore);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<int>(EventNameSaver.OnScoreChanged, UpdateScore);
            EventManager.Instance.RemoveListener<int>(EventNameSaver.OnBestScoreChanged, UpdateBestScore);
        }

        private void UpdateScore(int value)
        {
            worldScoreText.text = value.ToString("000");
        }

        private void UpdateBestScore(int value)
        {
            bestScoreText.text = value.ToString("000");
        }
    }
}
