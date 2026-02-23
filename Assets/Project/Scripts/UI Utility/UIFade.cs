using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFade : MonoBehaviour
{
    public float duration = 0.3f;
    public LeanTweenType ease = LeanTweenType.easeOutQuad;

    private CanvasGroup canvasGroup;

    public float FadeIn()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        LeanTween.alphaCanvas(canvasGroup, 1f, duration).setEase(ease);
        return duration;
    }

    public float FadeOut()
    {
        LeanTween.alphaCanvas(canvasGroup, 0f, duration).setEase(ease);
        return duration;
    }
}