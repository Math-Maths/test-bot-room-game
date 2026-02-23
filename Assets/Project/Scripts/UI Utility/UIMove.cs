using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIMove : MonoBehaviour
{
    public float duration = 0.3f;
    public Vector2 targetPosition;
    public LeanTweenType ease;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Play()
    {
        LeanTween.move(rect, targetPosition, duration).setEase(ease);
    }
}