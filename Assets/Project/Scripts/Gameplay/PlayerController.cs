using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TestBotRoom
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IInitiation
    {
        
        private PlayerInput _playerInput;
        private CharacterController _controller;

        private float _gravityValue = -9.81f;
        private bool _isGrounded;
        private bool _canMove = true;
        private Vector3 _playerVelocity;
        private string _currentAnimation;

        #region Input Actions
        private InputAction _move;
        private InputAction _jump;
        #endregion

        [Header("Player Settings")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpForce;
        [SerializeField] private Animator playerAnimator;

        public void OnInitiate()
        {
            _playerInput = GetComponent<PlayerInput>();
            _controller = GetComponent<CharacterController>();

            _move = _playerInput.actions["Move"];
            _jump = _playerInput.actions["Jump"];

            _canMove = false;
        }

        public void StartGamePlay()
        {
            transform.Translate(Vector3.zero, Space.World);
            _canMove = true;   
            ShotAnimation("Player Idle");
        }

        private void Update()
        {
            if(GameManager.Instance.CurrentGameState != GameState.Gameplay || !_canMove)
                return;

            ApplyMoveAndGravity();
        }

        private void ApplyMoveAndGravity()
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
                ShotAnimation("Player Jump Start");
            }

            _playerVelocity.y += _gravityValue * Time.deltaTime;

            Vector3 finalMove = move * moveSpeed + Vector3.up * _playerVelocity.y;
            _controller.Move(finalMove * Time.deltaTime);

            CheckAnimations();
        }

        private void CheckAnimations()
        {
            bool isMoving = _move.ReadValue<Vector2>() != Vector2.zero;

            if(_currentAnimation == "Player Jump End" || _currentAnimation == "Player Jump Start" || _currentAnimation == "Player Death")
                return;

            if(_currentAnimation == "Player Jump Air")
            {
                if(_isGrounded)
                    ShotAnimation("Player Jump End");
                return;
            }

            if(isMoving)
            {
                ShotAnimation("Player Walk");
            }
            else
            {
                ShotAnimation("Player Idle");
            }
        }

        public void ShotAnimation(string animationName, float delay = 0f)
        {
            if(delay > 0f)
            {
                StartCoroutine(WaitAndPlay());
            }
            else
            {
                ValidateAndPlay();
            }

            IEnumerator WaitAndPlay()
            {  
                yield return new WaitForSeconds(delay);
                ValidateAndPlay();
            }

            void ValidateAndPlay()
            {
                if(animationName == "")
                    CheckAnimations();
                else
                    playerAnimator.Play(animationName);

                if(_currentAnimation == animationName)
                    return;
                _currentAnimation = animationName;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Laser"))
            {
                _canMove = false;
                EventManager.Instance.Invoke(EventNameSaver.OnGameOver);
                ShotAnimation("Player Death");
            }
        }

    }
}