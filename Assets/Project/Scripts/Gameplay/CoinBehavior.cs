using UnityEngine;
using TestBotRoom.Utils;

namespace TestBotRoom.Gameplay
{
    public class CoinBehavior : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 50f;

        [Header("Floating")]
        [SerializeField] private float minHeightOffset = -0.2f;
        [SerializeField] private float maxHeightOffset = 0.2f;
        [SerializeField] private float floatSpeed = 1f;

        [Header("Spawn")]
        [SerializeField] private float scaleTime = 0.5f;

        private float _startY;
        private Collider _collider;

        private void Awake()
        {
            _startY = transform.position.y;
            _collider = GetComponent<Collider>();
        }

        private void Update()
        {
            Rotate();
            Float();
        }

        private void Rotate()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        private void Float()
        {
            float t = (Mathf.Sin(Time.time * floatSpeed) + 1f) * 0.5f; // 0 → 1
            float y = Mathf.Lerp(_startY + minHeightOffset, _startY + maxHeightOffset, t);

            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            DifficultyMultiplier.currentCoinCount++;
            EventManager.Instance.Invoke(EventNameSaver.OnCoinColleted);
        }

        public void SpawnAnimation()
        {
            if (_collider != null)
                _collider.enabled = false;

            transform.localScale = Vector3.zero;

            LeanTween.scale(gameObject, Vector3.one * 0.4f, scaleTime)
             .setEaseInOutBack()
             .setOnComplete(() =>
            {
                // Re-enable collider after animation
                if (_collider != null)
                    _collider.enabled = true;
            });
        }
    }
}
