using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Mathematics;
using Unity.VisualScripting;

namespace Pathless_Recreation
{
    public class ArrowSystem : MonoBehaviour
    {
        //Events
        public Action OnInputStart;
        public Action OnInputRelease;
        public Action<float> OnArrowRelease;
        public Action OnTargetHit;
        public Action OnTargetLost;
        
        public ParticleSystem wrongArrowParticle;
        public ParticleSystem correctArrowParticle;
        public Slider arrowChargeSlider;
        public ParticleSystem StaminaParticle;
        
        public float slowDownInterval = 0.5f;
        public float systemCooldownTime = 1f;
        public float chargeTime = 1f;
        public float assistTimePercent = 0.1f;
        private float chargeAmount;

        TargetSystem targetSystem;
        ArrowTarget lockedTarget;
        MovementControl movementControl;
        Animator anim;

        public bool isCharging = false;
        public bool releaseCooldown;
        bool isActive = true;
        Coroutine arrowSystemCooldown;


        private void Start()
        {
            targetSystem = GetComponent<TargetSystem>();
            movementControl = GetComponent<MovementControl>();
            anim = GetComponent<Animator>();

            movementControl.input.Player.Fire.performed += Fire_Start;
            movementControl.input.Player.Fire.canceled += Fire_Cancel;
        }

        #region Input

        private void Fire_Start(InputAction.CallbackContext context)
        {
            StartFire();
            Debug.Log("Fire started");
        }

        private void Fire_Cancel(InputAction.CallbackContext context)
        {
            CancelFire(true);
            Debug.Log("Fire cancelled");
        }

        #endregion

        private void Update()
        {
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            anim.SetBool("IsCharging", isCharging);
            anim.SetBool("ReleaseCooldown", releaseCooldown);
            anim.SetBool("IsRunning", movementControl.isRunning);
        }

        private void StartFire()
        {
            if (!isActive) return;
            if (targetSystem.currentTarget == null || !targetSystem.currentTarget.isAvailable) return;

            OnInputStart?.Invoke();
            
            isCharging = true;
            DOVirtual.Float(0, 1, chargeTime, SetChargeAmount).SetId(0);
        }

        private void SetChargeAmount(float value)
        {
            chargeAmount = value;
            arrowChargeSlider.value = chargeAmount;
        }

        public void CancelFire(bool shootArrow)
        {
            if (shootArrow){
                if(!isCharging) return;
                if(targetSystem.currentTarget == null || !targetSystem.currentTarget.isAvailable) return;
                
                OnInputRelease?.Invoke();
                CheckArrowRelease();
                
            }
            
            OnTargetLost?.Invoke();
            
            isCharging = false;
            DOTween.Kill(0);
            SetChargeAmount(0);
        }

        private void CheckArrowRelease()
        {
            DisableSystemForPeriod();

            if (HalfTime())
                chargeAmount = 0.5f;
            else if(FullTime())
                chargeAmount = 1f;
            
            OnArrowRelease?.Invoke(chargeAmount);
            
            if (HalfTime() || FullTime()){

                if (HalfTime())
                    StartCoroutine(SlowTime());
                ShootCorrectArrow();
            }
            else{
                ShootWrongArrow();
            }
        }

        private IEnumerator SlowTime()
        {
            float scale = 0.2f;
            Time.timeScale = scale;
            
            yield return new WaitForSecondsRealtime(slowDownInterval * scale);
            Time.timeScale = 1f;
        }

        private bool HalfTime() => chargeAmount >= .5f - assistTimePercent && chargeAmount <= .5f + assistTimePercent;
        private bool FullTime() => chargeAmount >= 1 - assistTimePercent;

        private void ShootWrongArrow()
        {
            lockedTarget = targetSystem.currentTarget;
            wrongArrowParticle.transform.position = transform.position + Vector3.up * 0.7f;
            wrongArrowParticle.transform.LookAt(lockedTarget.transform.position);
            wrongArrowParticle.Play();
            //print($"wrong arrow");
        }

        private void ShootCorrectArrow()
        {
            lockedTarget = targetSystem.currentTarget;
            correctArrowParticle.transform.position = lockedTarget.transform.position;
            var shape = correctArrowParticle.shape;
            shape.position = correctArrowParticle.transform.InverseTransformPoint(transform.position);
            correctArrowParticle.Play();
            
            //print($"correct arrow");
        }

        private void DisableSystemForPeriod()
        {
            isActive = false;
            if (arrowSystemCooldown != null)
                StopCoroutine(arrowSystemCooldown);
            arrowSystemCooldown = StartCoroutine(SystemCooldown());

            IEnumerator SystemCooldown()
            {
                releaseCooldown = true;
                yield return new WaitForSeconds(systemCooldownTime);
                releaseCooldown = false;
                
                isActive = true;
            }
        }

        public void TargetHit(Vector3 arrowDir)
        {
            OnTargetHit?.Invoke();

            releaseCooldown = false;
            isActive = true;
            lockedTarget.DisableTarget(arrowDir);
            
            //Release stamina part
            var shape = StaminaParticle.shape;
            shape.position = transform.InverseTransformPoint(lockedTarget.transform.position);
            StaminaParticle.Play();
        }
    }
}