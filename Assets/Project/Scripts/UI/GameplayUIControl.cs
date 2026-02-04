using TMPro;
using UnityEngine;
using System.Threading.Tasks;

namespace TestBotRoom.UI
{
    public class GameplayUIControl : MonoBehaviour, IInitiation
    {
        [Header("World UI Elements")]
        [SerializeField] private TMP_Text currentScoreText_World;

        [Space(10)]
        [Header("End Screen UI Elements")]
        [SerializeField] private GameObject endScreenPanel;
        [SerializeField] private TMP_Text currentScoreText;
        [SerializeField] private TMP_Text bestScoreText;

        [Space(10)]
        [Header("UI Controls")]
        [SerializeField] private GameObject controlsPanel;

        [Space(10)]
        [Header("Initial Gameplay Screen")]
        [SerializeField] private TMP_Text startDelayText;

        public void OnInitiate()
        {
            EventManager.Instance.AddListener<int>(EventNameSaver.OnScoreChanged, UpdateScore);
            EventManager.Instance.AddListener<int>(EventNameSaver.OnBestScoreChanged, UpdateBestScore);
            EventManager.Instance.AddListener(EventNameSaver.OnGameOver, DisableControls);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<int>(EventNameSaver.OnScoreChanged, UpdateScore);
            EventManager.Instance.RemoveListener<int>(EventNameSaver.OnBestScoreChanged, UpdateBestScore);
            EventManager.Instance.RemoveListener(EventNameSaver.OnGameOver, DisableControls);
        }

        public async Task ShowCountdown(float seconds)
        {
            startDelayText.gameObject.SetActive(true);

            int remaining = Mathf.CeilToInt(seconds);

            while (remaining > 0)
            {
                startDelayText.text = remaining.ToString();
                await Task.Delay(1000);
                remaining--;
            }

            startDelayText.text = "GO!";
            await Task.Delay(500);

            startDelayText.gameObject.SetActive(false);
            controlsPanel.SetActive(true);
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

        public void DisableControls()
        {
            controlsPanel.SetActive(false);
        }
    }
}
