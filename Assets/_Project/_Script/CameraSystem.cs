using System;
using Pathless_Recreation;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class CameraSystem : MonoBehaviour
{
    MovementControl input;
    ArrowSystem arrowSystem;
    TargetSystem targetSystem;
    
    [Header("Camera Settings")]
    [SerializeField] CinemachineCamera thirdPersonCam;
    [SerializeField] CinemachineOrbitalFollow orbitController;
    [SerializeField] CinemachineRotationComposer rotationComposer;
    [SerializeField] CinemachineInputAxisController cameraInputProvider;
    [SerializeField] CinemachineImpulseSource defaultImpulse;
    [SerializeField] CinemachineImpulseSource perfectImpulse;

    [Header("Configs")] 
    [SerializeField] float boostFov = 100f;
    [SerializeField] float runFov = 60f;
    [SerializeField] float defaultFov = 50f;
    [SerializeField] float boostOrbitRadius = 2.5f;
    [SerializeField] float runOrbitRadius = 3.5f;
    [SerializeField] float defaultOrbitRadius = 15f;
    [SerializeField] private float cameraOffset = 0.25f;
    private float originalOffset;
    [SerializeField] private float cameraOffsetLerp = 1f;
    private float originalCameraOffsetLerp;

    private void Awake()
    {
        input = GetComponent<MovementControl>();
        arrowSystem = GetComponent<ArrowSystem>();
        targetSystem = GetComponent<TargetSystem>();

        
    }

    #region Event Sub/Unsub

    private void OnEnable()
    {
        arrowSystem.OnInputStart += LockCamera;
        arrowSystem.OnInputRelease += UnlockCamera;
        arrowSystem.OnTargetLost += UnlockCamera;
        arrowSystem.OnArrowRelease += PerfectShake;
        arrowSystem.OnTargetHit += Shake;
    }

    private void OnDisable()
    {
        arrowSystem.OnInputStart -= LockCamera;
        arrowSystem.OnInputRelease -= UnlockCamera;
        arrowSystem.OnTargetLost -= UnlockCamera;
        arrowSystem.OnArrowRelease -= PerfectShake;
        arrowSystem.OnTargetHit -= Shake;
    }

    #endregion

    private void Update()
    {
        bool isBoosting = input.isBoosting;
        bool isRunning = input.isRunning;
        bool finishedBoost = input.isBoostJustFinished;
        
        float fov = isRunning ? (isBoosting ? boostFov : runFov) : defaultFov;
        float lerpAmount = finishedBoost ? 0.006f : 0.01f;
        
        thirdPersonCam.Lens.FieldOfView = Mathf.Lerp(thirdPersonCam.Lens.FieldOfView, fov, lerpAmount);

        for (int i = 0; i < 3; i++){
            float newRadius = isBoosting ? boostOrbitRadius : (isRunning ? runOrbitRadius : defaultOrbitRadius);

            Cinemachine3OrbitRig.Orbit orbit;
            
            if(i == 0)
                orbit = orbitController.Orbits.Bottom;
            else if(i == 1)
                orbit = orbitController.Orbits.Center;
            else{
                orbit = orbitController.Orbits.Top;
            }
            
            orbit.Radius = Mathf.Lerp(orbit.Radius, newRadius, lerpAmount);
            
            float targetScreenPos = targetSystem.lerpedTargetPos.x;
            float characterScreenPos = Camera.main.WorldToScreenPoint(transform.position).x;

            cameraOffset = arrowSystem.isCharging ? originalOffset * 3 : originalOffset;
            float targetCharacterDistance = ExtensionMethods.Remap(targetScreenPos - characterScreenPos, -800, 800, -cameraOffset, cameraOffset);
            targetCharacterDistance = Mathf.Clamp(targetCharacterDistance, -cameraOffset, cameraOffset);

            cameraOffsetLerp = originalCameraOffsetLerp;
            rotationComposer.Composition.ScreenPosition.x = Mathf.Lerp(rotationComposer.Composition.ScreenPosition.x, isRunning ? .5f - targetCharacterDistance : .5f, cameraOffsetLerp * Time.deltaTime);
        }
    }

    private void LockCamera()
    {
        if(cameraInputProvider != null)
            cameraInputProvider.enabled = false;
    }

    private void UnlockCamera()
    {
        if(cameraInputProvider != null)
            cameraInputProvider.enabled = true;
    }

    private void Shake()
    {
        if(input.isRunning || input.isHoldingRunInput)
            defaultImpulse.GenerateImpulse();
    }

    private void PerfectShake(float chargeAmount)
    {
        if(chargeAmount == 0.5f)
            perfectImpulse.GenerateImpulse();
    }
}