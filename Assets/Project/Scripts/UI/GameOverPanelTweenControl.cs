using UnityEngine;

namespace TestBotRoom.UI
{
    public class GameOverPanelTweenControl : MonoBehaviour
    {
        [SerializeField] private UIPopup panelPopup;
        [SerializeField] private UIScale menuButton;
        [SerializeField] private UIScale resetButton;
        [SerializeField] private UIScale continueButton;
        [SerializeField] private UIScale scoreText;
        [SerializeField] private UIScale bestScoreText;
        [SerializeField] private UIScale avatarIcon;
        [SerializeField] private ScoreCounter scoreCounter;

        private float showDuration;
        private RectTransform rect;

        public void PlayGameOverAnimation()
        {
            rect = GetComponent<RectTransform>();
            rect.localScale = Vector3.zero;
            showDuration = panelPopup.Show();

            menuButton.gameObject.SetActive(true);
            PlayWithDelay(showDuration, menuButton);
            avatarIcon.gameObject.SetActive(true);
            PlayWithDelay(showDuration, avatarIcon);
            continueButton.gameObject.SetActive(true);
            PlayWithDelay(showDuration + 0.2f, continueButton);
            resetButton.gameObject.SetActive(true);
            PlayWithDelay(showDuration + 0.4f, resetButton);
            scoreText.gameObject.SetActive(true);
            PlayWithDelay(showDuration + 0.6f, scoreText);
            scoreCounter.AnimateScore(scoreText.duration + showDuration + 0.6f);
            bestScoreText.gameObject.SetActive(true);
            PlayWithDelay(showDuration + 0.8f, bestScoreText);
        }

        private void PlayWithDelay(float delay, UIScale uiScale)
        {
            LeanTween.delayedCall(delay, () => uiScale.Play());
        }

        public void Hide()
        {
            menuButton.gameObject.SetActive(false);
            menuButton.ResetScale();
            avatarIcon.gameObject.SetActive(false);
            avatarIcon.ResetScale();
            continueButton.gameObject.SetActive(false);
            continueButton.ResetScale();
            resetButton.gameObject.SetActive(false);
            resetButton.ResetScale();
            scoreText.gameObject.SetActive(false);
            scoreText.ResetScale();
            bestScoreText.gameObject.SetActive(false);
            bestScoreText.ResetScale();
            gameObject.SetActive(false);
        }
    }
}