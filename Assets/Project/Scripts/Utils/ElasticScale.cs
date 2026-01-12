using System.Collections;
using UnityEngine;

public class ElasticScale : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float scaleMultiplier = 1.3f;
    [Range(0f, 0.5f)]
    [SerializeField] private float earlyStart = 0.2f;


    private Vector3 _originalScale;
    private Coroutine _currentRoutine;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// Plays the elastic scale animation using the given duration.
    /// </summary>
    public void Play(float duration)
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(AnimateScale(duration));
    }

    private IEnumerator AnimateScale(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = Mathf.Clamp01((t + earlyStart) / (1f + earlyStart));

            //float easeValue = EaseInElastic(t);
            //float easeValue = EaseOutElastic(t);

            float curve = ElasticImpulse(t);

            float scale = 1f + curve * (scaleMultiplier - 1f);

            transform.localScale = new Vector3(
                _originalScale.x,
                _originalScale.y * scale,
                _originalScale.z * scale
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Instantly reset scale
        transform.localScale = _originalScale;
        _currentRoutine = null;
    }

    private float EaseInElastic(float x)
    {
        const float c4 = (2f * Mathf.PI) / 3f;

        if (x == 0f) return 0f;
        if (x == 1f) return 1f;

        return -Mathf.Pow(2f, 10f * x - 10f) *
               Mathf.Sin((x * 10f - 10.75f) * c4);
    }

    private float EaseOutElastic(float x)
    {
        const float c4 = (2f * Mathf.PI) / 3f;

        if (x == 0f) return 0f;
        if (x == 1f) return 1f;

        return Mathf.Pow(2f, -10f * x) *
               Mathf.Sin((x * 10f - 0.75f) * c4) + 1f;
    }

    private float ElasticImpulse(float x)
    {
        // Tunable values
        const float growEnd = 0.55f;     // When growth ends
        const float frequency = 14f;     // Oscillation speed
        const float damping = 6f;        // How fast oscillation dies
        const float overshoot = 0.25f;   // How far it goes beyond max

        if (x < growEnd)
        {
            // Normalize time inside growth phase
            float t = x / growEnd;

            // Fast growth
            float baseGrowth = Mathf.SmoothStep(0f, 1f, t);

            // Elastic oscillation
            float oscillation =
                Mathf.Sin(t * frequency) *
                Mathf.Exp(-t * damping) *
                overshoot;

            return baseGrowth + oscillation;
        }
        else
        {
            // Snap back fast
            float t = (x - growEnd) / (1f - growEnd);

            return Mathf.Lerp(1f, 0f, t);
        }
    }
}