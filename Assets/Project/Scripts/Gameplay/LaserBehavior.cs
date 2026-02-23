using UnityEngine;

namespace TestBotRoom.Gameplay
{
    public class LaserBehavior : MonoBehaviour, IPoolable
    {
        [SerializeField] private float moveSpeed = 2f;

        private bool _initialized = false;

        private void Update()
        {
            if(_initialized)
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);

            float limitDistanteX = Mathf.Abs(transform.position.x);
            float limitDistanteZ = Mathf.Abs(transform.position.z);

            if(limitDistanteX > 9 || limitDistanteZ > 9)
            {
                PoolService.Instance.Despawn("LaserPool", gameObject);
            }
        }

        public void InitializeLaser(Vector3 forwardDir, float speed)
        {
            transform.forward = forwardDir;
            moveSpeed = speed;
            _initialized = true;
        }

        public void OnDespawned()
        {
            
        }

        public void OnSpawned()
        {
            
        }

    }
}