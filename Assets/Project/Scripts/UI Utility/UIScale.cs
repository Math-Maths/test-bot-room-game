using UnityEngine;

public class UIScale : MonoBehaviour
{
    public float duration = 0.3f;
    public Vector3 targetScale = Vector3.one;
    public LeanTweenType ease;

    public float Play()
    {
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, targetScale, duration).setEase(ease);
        return duration;
    }

    public void ResetScale()
    {
        transform.localScale = Vector3.zero;
    }
}