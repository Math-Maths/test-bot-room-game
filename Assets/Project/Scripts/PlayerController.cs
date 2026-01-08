using UnityEngine;
using UnityEngine.InputSystem;

namespace TestBotRoom
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        
        private PlayerInput _playerInput;
        private CharacterController _controller;

        private float _gravityValue = -9.81f;
        private bool _isGrounded;
        private Vector3 _playerVelocity;

        #region Input Actions
        private InputAction _move;
        private InputAction _jump;
        #endregion

        [Header("Player Settings")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpForce;

        private bool isGrounded;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _controller = GetComponent<CharacterController>();

            _move = _playerInput.actions["Move"];
            _jump = _playerInput.actions["Jump"];
        }

        private void Update()
        {
            _isGrounded = _controller.isGrounded;

            if(_isGrounded)
            {
                if(_playerVelocity.y < -2f)
                    _playerVelocity.y = -2f;
            }

            Vector2 moveInput = _move.ReadValue<Vector2>();
            Vector3 move = new Vector3(moveInput.x, 0 , moveInput.y);
            move = Vector3.ClampMagnitude(move, 1f);

            if(move != Vector3.zero)
                transform.forward = move;

            if(_isGrounded && _jump.WasPressedThisFrame())
            {
                _playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * _gravityValue);
            }

            _playerVelocity.y += _gravityValue * Time.deltaTime;

            Vector3 finalMove = move * moveSpeed + Vector3.up * _playerVelocity.y;
            _controller.Move(finalMove * Time.deltaTime);
        }

    }
}