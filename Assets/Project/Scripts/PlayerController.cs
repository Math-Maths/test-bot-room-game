using UnityEngine;
using UnityEngine.InputSystem;

namespace TestBotRoom
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        
        private PlayerInput _playerInput;
        private Rigidbody _playerRb;

        #region Input Actions
        private InputAction _move;
        private InputAction _jump;
        #endregion

        [Header("Player Settings")]
        [SerializeField] private float moveSpeed;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerRb = GetComponent<Rigidbody>();

            _move = _playerInput.actions["Move"];
            _jump = _playerInput.actions["Jump"];
        }

        private void Update()
        {
            Vector2 rawMove = _move.ReadValue<Vector2>();
            Vector3 velocity = new Vector3(rawMove.x, 0, rawMove.y) * moveSpeed * Time.deltaTime;
            Vector3 moveDir = velocity.normalized;

            _playerRb.transform.position += velocity;
            
            if (moveDir != Vector3.zero)
            {
                _playerRb.transform.rotation = Quaternion.LookRotation(moveDir);
            }
        }

    }
}