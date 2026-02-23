using UnityEngine;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    private TMP_Text scoreText;

    [Header("Animation Settings")]
    public float duration = 1.5f;
    public LeanTweenType easeType = LeanTweenType.easeOutQuad;

    private int targetScore = 0;
    private LTDescr tween;

    public void SetScore(int score)
    {
        if (scoreText == null)
            scoreText = GetComponent<TMP_Text>();

        LeanTween.cancel(gameObject);

        targetScore = score;

        scoreText.text = targetScore.ToString("000");
    }

    public void AnimateScore(float delay = 0f)
    {
        scoreText.text = "000";

        LeanTween.cancel(gameObject);

        if (delay > 0f)
        {
            LeanTween.delayedCall(gameObject, delay, () => StartAnimation());
        }
        else
        {
            StartAnimation();
        }
    }

    private void StartAnimation()
    {
        int startValue = 0;

        tween = LeanTween.value(gameObject, startValue, targetScore, duration)
            .setEase(easeType)
            .setOnUpdate((float val) =>
            {
                int displayValue = Mathf.RoundToInt(val);
                scoreText.text = displayValue.ToString("000");
            })
            .setOnComplete(() =>
            {
                scoreText.text = targetScore.ToString("000");
            });
    }
}