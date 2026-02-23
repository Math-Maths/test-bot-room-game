using UnityEngine;

public class UIOscillate : MonoBehaviour
{
    public float distance = 20f;
    public float duration = 1f;

    void OnEnable()
    {
        float randomDelay = Random.Range(0f, duration);

        LeanTween.moveLocalY(gameObject, transform.localPosition.y + distance, duration)
            .setEaseInOutSine()
            .setDelay(randomDelay)
            .setLoopPingPong();
    }
}