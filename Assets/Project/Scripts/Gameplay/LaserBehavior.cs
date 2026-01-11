using UnityEngine;

namespace TestBotRoom
{
    public class LaserBehavior : MonoBehaviour
    {
            
        [SerializeField] private float moveSpeed = 2f;

        private bool _initialized = false;

        private void Update()
        {
            if(_initialized)
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        }

        public void InitializeLaser(Vector3 forwardDir, float speed)
        {
            transform.forward = forwardDir;
            moveSpeed = speed;
            _initialized = true;
            Destroy(gameObject, 10f);
        }

    }
}