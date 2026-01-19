using TMPro;
using UnityEngine;

namespace TestBotRoom.UI
{
    public class GameplayUIControl : MonoBehaviour, IInitiation
    {
        [Header("World UI Elements")]
        [SerializeField] private TMP_Text currentScoreText_World;

        [Header("End Screen UI Elements")]
        [SerializeField] private GameObject endScreenPanel;
        [SerializeField] private TMP_Text currentScoreText;
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
            currentScoreText_World.text = value.ToString("000");
        }

        private void UpdateBestScore(int value)
        {
            bestScoreText.text = value.ToString("000");
        }

        public void CallEvent(string eventName)
        {
            EventManager.Instance.Invoke(eventName);
        }

        public void ShowEndScreen(int finalScore)
        {
            currentScoreText.text = finalScore.ToString("000");
            endScreenPanel.SetActive(true);
        }
    }
}
