using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.Serialization;

namespace Pathless_Recreation
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementControl : MonoBehaviour
    {
        [Header("Movement Settings")] public float movementSpeed = 7f;
        public float rotationSpeed = 5f;
        public float runAcceleration = 2f;
        public float accelerationMultiplier = 1f;
        public float accelerationLerp = 0.5f;

        [Header("Collision Setting")] public LayerMask groundLayerMask;

        [Header("Jump Setting")] public float jumpSpeed = 8f;
        public float jumpHoldTime = 0.2f;
        private float jumpTimer;
        public float gravity = 9.81f;
        public float gravityMultiplier = 2f;

        private bool isGrounded;
        private float currentAcceleration;
        private float verticalVelocity;

        //Input Containers
        public bool isRunning;
        private bool holdingJumpKey;
        private Vector2 moveInput;

        public PlayerInput input;
        Animator animator;
        CharacterController controller;

        #region event Management

        private void OnEnable()
        {
            input.Player.Jump.performed += JumpAction_started;
            input.Player.Jump.canceled += JumpAction_canceled;
        }

        private void OnDisable()
        {
            input.Player.Jump.performed -= JumpAction_started;
            input.Player.Jump.canceled -= JumpAction_canceled;
        }

        private void JumpAction_started(InputAction.CallbackContext context)
        {
            if (controller.isGrounded)
            {
                holdingJumpKey = true;
                animator.SetTrigger("Jump");
                jumpTimer = 0f;
            }
        }

        private void JumpAction_canceled(InputAction.CallbackContext context)
        {
            holdingJumpKey = false;
        }

        #endregion

        private void Awake()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();

            input = new PlayerInput();
            input.Player.Enable();
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            moveInput = input.Player.Move.ReadValue<Vector2>();
            isRunning = input.Player.Sprint.IsPressed();

            HandleMovement();
            Jump();
        }

        private void HandleMovement()
        {
            float inputSqrMagnitude = moveInput.sqrMagnitude;

            currentAcceleration = Mathf.Lerp(currentAcceleration,
                isRunning ? (runAcceleration * accelerationMultiplier) : 1f,
                Time.deltaTime * accelerationLerp);

            if (inputSqrMagnitude > 0.1f)
            {

                animator.SetFloat("Velocity", inputSqrMagnitude * currentAcceleration, 0.1f, Time.deltaTime);
                PlayerMoveAndRotate();
            }
            else
            {
                animator.SetFloat("Velocity", currentAcceleration * inputSqrMagnitude, 0.1f, Time.deltaTime);
            }

            //Debug.Log($"isRunning: {isRunning}");
            //Debug.Log($"Input Square Magnitude: {moveInput.sqrMagnitude} || current Acceleration: {currentAcceleration}");
        }

        private void PlayerMoveAndRotate()
        {
            var camera = Camera.main.transform;
            Vector3 forward = camera.forward;
            Vector3 right = camera.right;
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();


            Vector3 desiredMoveDirection = forward * moveInput.y + right * moveInput.x;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection),
                rotationSpeed * currentAcceleration);
            controller.Move(desiredMoveDirection * Time.deltaTime * (movementSpeed * runAcceleration));

        }

        private void Jump()
        {

            if (holdingJumpKey)
            {
                jumpTimer += Time.deltaTime;
                float jumpDurationPercent = Mathf.Clamp01(jumpTimer / jumpHoldTime);
                verticalVelocity = jumpSpeed * jumpDurationPercent;
                if (verticalVelocity >= jumpSpeed)
                    holdingJumpKey = false;
            }
            else
            {
                if (!isGrounded)
                    verticalVelocity -= gravity * gravityMultiplier * Time.deltaTime;
            }

            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        private void GroundCheck()
        {
            Vector3 origin = transform.position + transform.up * 0.05f;
            isGrounded = Physics.Raycast(origin, Vector3.down, 0.2f, groundLayerMask);
            animator.SetBool("isGrounded", isGrounded);
        }
    }
}