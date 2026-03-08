using UnityEngine;

public class UIOscillate : MonoBehaviour
{
    public float distance = 20f;
    public float duration = 1f;

    private float startYPos;

    void OnEnable()
    {
        if(startYPos == 0)
            startYPos = transform.localPosition.y;

        float randomDelay = Random.Range(0f, duration);

        transform.localPosition = new Vector3(transform.localPosition.x, startYPos, transform.localPosition.z);

        LeanTween.moveLocalY(gameObject, startYPos + distance, duration)
            .setEaseInOutSine()
            .setLoopPingPong()
            .setDelay(randomDelay);
    }

    void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }
}