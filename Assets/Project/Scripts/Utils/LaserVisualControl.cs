using System.Collections;
using UnityEngine;

public class LaserVisualControl : MonoBehaviour
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
    [Tooltip("0-1: quanto da animação até atingir a cor máxima")]
    [SerializeField] private float timeUntilMaxColor = 0.8f;

    [Header("Emission Settings")]
    [SerializeField] private Color baseEmission = Color.black;
    [SerializeField] private Color warningEmission = new Color(3f, 0f, 0f); // HDR recomendado

    private Vector3 _originalScale;
    private Coroutine _currentRoutine;
    private MaterialPropertyBlock _mpb;

    // Shader IDs
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int ChargeID = Shader.PropertyToID("_Charge");

    private void Awake()
    {
        _originalScale = transform.localScale;
        _mpb = new MaterialPropertyBlock();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"{name} - Target Renderer não atribuído.");
            return;
        }

        ApplyVisual(0f, baseColor, baseEmission);
    }

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
            // ======================
            // SCALE (elastic)
            // ======================
            float t = elapsed / duration;
            t = Mathf.Clamp01((t + earlyStart) / (1f + earlyStart));

            float curve = ElasticImpulse(t);

            float scale = 1f + curve * (scaleMultiplier - 1f);

            transform.localScale = new Vector3(
                _originalScale.x,
                _originalScale.y * scale,
                _originalScale.z * scale
            );

            // ======================
            // COLOR + EMISSION (linear)
            // ======================
            float colorDuration = duration * timeUntilMaxColor;
            float colorT = Mathf.Clamp01(elapsed / colorDuration);
            colorT = Mathf.SmoothStep(0f, 1f, colorT);

            Color currentBaseColor = Color.Lerp(baseColor, warningColor, colorT);
            Color currentEmission = Color.Lerp(baseEmission, warningEmission, colorT);

            // ======================
            // CHARGE (controla o shader)
            // ======================
            float charge = Mathf.Pow(Mathf.Clamp01(elapsed / duration), 2f);
            charge = Mathf.SmoothStep(0f, 1f, charge);

            // Aplicar tudo
            ApplyVisual(charge, currentBaseColor, currentEmission);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset
        transform.localScale = _originalScale;
        ApplyVisual(0f, baseColor, baseEmission);

        _currentRoutine = null;
    }

    private void ApplyVisual(float charge, Color baseCol, Color emissionCol)
    {
        if (targetRenderer == null)
            return;

        targetRenderer.GetPropertyBlock(_mpb);

        _mpb.SetColor(BaseColorID, baseCol);
        _mpb.SetColor(EmissionColorID, emissionCol);
        _mpb.SetFloat(ChargeID, charge);

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
