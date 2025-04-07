using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Pathless_Recreation
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementControl : MonoBehaviour
    {
        public Action OnBoostStart;
        
        [Header("Movement Settings")] 
        public float movementSpeed = 7f;
        public float rotationSpeed = 5f;
        public float runAcceleration = 2f;
        public float accelerationMultiplier = 1f;
        public float defaultAccelerationLerp = 0.5f;
        public float boostCooldownLerp = 0.06f;

        [Header("Collision Setting")] public LayerMask groundLayerMask;

        [Header("Jump Setting")] 
        public float jumpSpeed = 8f;
        public float jumpHoldTime = 0.2f;
        private float jumpTimer;
        public float gravity = 9.81f;
        public float gravityMultiplier = 2f;
        
        [Header("Boost Settings")]
        public float boostCooldownTime = 0.2f;
        public float boostAccelerationMultiplier = 1.6f;

        private bool isGrounded;
        private float currentAcceleration;
        private float verticalVelocity;

        //Input Containers
        public bool isBoosting;
        public bool isBoostJustFinished;
        public bool isRunning;
        public bool isHoldingRunInput;
        [FormerlySerializedAs("holdingJumpKey")] public bool isJumping;
        private Vector2 moveInput;

        BoostSystem boostSystem;
        ArrowSystem arrowSystem;
        
        public PlayerInput input;
        Animator animator;
        CharacterController controller;
        Coroutine boostCoroutine;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
            boostSystem = GetComponent<BoostSystem>();
            arrowSystem = GetComponent<ArrowSystem>();

            input = new PlayerInput();
            input.Player.Enable();
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        #region event Management

        private void OnEnable()
        {
            input.Player.Jump.performed += JumpAction_Started;
            input.Player.Jump.canceled += JumpAction_Canceled;
            input.Player.Sprint.performed += RunAction_Started;
            input.Player.Sprint.canceled += RunAction_Canceled;

            arrowSystem.OnTargetHit += Boost;
        }


        private void OnDisable()
        {
            input.Player.Jump.performed -= JumpAction_Started;
            input.Player.Jump.canceled -= JumpAction_Canceled;
            input.Player.Sprint.performed -= RunAction_Started;
            input.Player.Sprint.canceled -= RunAction_Canceled;
            
            arrowSystem.OnTargetHit -= Boost;
        }

        #endregion

        private void Update()
        {
            GroundCheck();
            PlayerMovement();
        }

        private void PlayerMovement()
        {
            moveInput = input.Player.Move.ReadValue<Vector2>();

            HandleGroundMovement();
            Jump();
        }

        private void HandleGroundMovement()
        {
            float inputSqrMagnitude = moveInput.sqrMagnitude;

            float lerp = isBoostJustFinished ? boostCooldownLerp : defaultAccelerationLerp;
            
            currentAcceleration = Mathf.Lerp(currentAcceleration,
                isRunning ? (runAcceleration * accelerationMultiplier) : 1f,
                Time.deltaTime * lerp);

            if (inputSqrMagnitude > 0.1f)
            {
                
                animator.SetFloat("InputMagnitude", inputSqrMagnitude * currentAcceleration, 0.1f, Time.deltaTime);
                PlayerMoveAndRotate();
            }
            else
            {
                animator.SetFloat("InputMagnitude", currentAcceleration * inputSqrMagnitude, 0.1f, Time.deltaTime);
            }

            //Debug.Log($"isRunning: {isRunning}");
            Debug.Log($"Input Square Magnitude: {inputSqrMagnitude} || current Acceleration: {currentAcceleration}");
            Debug.Log($"Velocity: {inputSqrMagnitude * currentAcceleration }");
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

            if (isJumping)
            {
                jumpTimer += Time.deltaTime;
                float jumpDurationPercent = Mathf.Clamp01(jumpTimer / jumpHoldTime);
                verticalVelocity = jumpSpeed * jumpDurationPercent;
                if (verticalVelocity >= jumpSpeed)
                    isJumping = false;
            }
            else
            {
                if (!isGrounded)
                    verticalVelocity -= gravity * gravityMultiplier * Time.deltaTime;
            }

            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime); 
        }

        private void Boost() 
        {
            if (!isHoldingRunInput) return;
            if(!isRunning && moveInput.magnitude <= 0f) return;

            isBoostJustFinished = false;
            
            OnBoostStart?.Invoke();
            
            if(!isGrounded)
                animator.SetTrigger("Flip");
            
            if(boostCoroutine != null)
                StopCoroutine(boostCoroutine);

            boostCoroutine = StartCoroutine(BoostCoroutine());

            IEnumerator BoostCoroutine()
            {
                if (!isGrounded)
                    isJumping = true;
                
                isBoosting = true;
                accelerationMultiplier = boostAccelerationMultiplier;
                yield return new WaitForSeconds(boostCooldownTime);
                
                isBoosting = false;
                accelerationMultiplier = 1f;
                isBoostJustFinished = true;

                yield return new WaitForSeconds(1f);
                
                isBoostJustFinished = false;
            }
        }

        private void GroundCheck()
        {
            Vector3 origin = transform.position + transform.up * 0.05f;
            isGrounded = Physics.Raycast(origin, Vector3.down, 0.2f, groundLayerMask);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("GroundedValue", isGrounded ? 0 : 1, .1f, Time.deltaTime);
        }
        
        #region Input Handler

        private void RunAction_Started(InputAction.CallbackContext obj)
        {
            isHoldingRunInput = true;

            isRunning = CanRun() && moveInput.magnitude > 0;
            
        }

        private bool CanRun()
        {
            return boostSystem != null && boostSystem.boostAmount > 0;
        }

        private void RunAction_Canceled(InputAction.CallbackContext obj)
        {
            isHoldingRunInput = false;
            isRunning = false;
        }
        
        private void JumpAction_Started(InputAction.CallbackContext context)
        {
            if (isGrounded)
            {
                isJumping = true;
                animator.SetTrigger("Jump");
                jumpTimer = 0f;
            }
        }

        private void JumpAction_Canceled(InputAction.CallbackContext context)
        {
            isJumping = false;
            jumpTimer = 0f;
        }
        
        #endregion
    }
}