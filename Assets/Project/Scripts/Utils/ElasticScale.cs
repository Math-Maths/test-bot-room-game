using System.Collections;
using UnityEngine;

public class ElasticScale : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Scale Settings")]
    [SerializeField] private float scaleMultiplier = 1.3f;
    [Range(0f, 0.5f)]
    [SerializeField] private float earlyStart = 0.2f;

    [Header("Color Settings")]
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float timeUntilMaxColor;

    [Header("Emission Settings")]
    [SerializeField] private Color baseEmission = Color.black;
    [SerializeField] private Color warningEmission = Color.red;

    private Vector3 _originalScale;
    private Coroutine _currentRoutine;

    private MaterialPropertyBlock _mpb;

    // Shader property ID (URP)
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _originalScale = transform.localScale;

        _mpb = new MaterialPropertyBlock();

        // Segurança
        if (targetRenderer == null)
        {
            Debug.LogWarning($"{name} - Target Renderer não atribuído.");
            return;
        }

        ApplyColor(baseColor, baseEmission);
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

            float curve = ElasticImpulse(t);

            // SCALE
            float scale = 1f + curve * (scaleMultiplier - 1f);

            transform.localScale = new Vector3(
                _originalScale.x,
                _originalScale.y * scale,
                _originalScale.z * scale
            );

            // COLOR (progressivo, independente da elasticidade)
            float colorDuration = duration * timeUntilMaxColor;
            float colorT = Mathf.Clamp01(elapsed / colorDuration);
            colorT = Mathf.SmoothStep(0f, 1f, colorT);

            // Base color
            Color currentBaseColor = Color.Lerp(baseColor, warningColor, colorT);

            // Emission color
            Color currentEmission = Color.Lerp(baseEmission, warningEmission, colorT);

            ApplyColor(currentBaseColor, currentEmission);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset
        transform.localScale = _originalScale;
        ApplyColor(baseColor, baseEmission);

        _currentRoutine = null;
    }

    private void ApplyColor(Color baseCol, Color emissionCol)
    {
        if (targetRenderer == null)
            return;

        targetRenderer.GetPropertyBlock(_mpb);

        _mpb.SetColor(BaseColorID, baseCol);
        _mpb.SetColor(EmissionColorID, emissionCol);

        targetRenderer.SetPropertyBlock(_mpb);
    }

    private float ElasticImpulse(float x)
    {
        const float growEnd = 0.55f;
        const float frequency = 14f;
        const float damping = 6f;
        const float overshoot = 0.25f;

        if (x < growEnd)
        {
            float t = x / growEnd;

            float baseGrowth = Mathf.SmoothStep(0f, 1f, t);

            float oscillation =
                Mathf.Sin(t * frequency) *
                Mathf.Exp(-t * damping) *
                overshoot;

            return baseGrowth + oscillation;
        }
        else
        {
            float t = (x - growEnd) / (1f - growEnd);

            return Mathf.Lerp(1f, 0f, t);
        }
    }
}
