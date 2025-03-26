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
        public ParticleSystem wrongArrowParticle;
        public ParticleSystem correctArrowParticle;
        public Slider arrowChargeSlider;
        
        public float systemCooldownTime = 1f;
        public float chargeTime = 1f;
        public float assistTimePercent = 0.1f;
        private float chargeAmount;

        TargetSystem targetSystem;
        ArrowTarget lockedTarget;
        MovementControl movementControl;

        public bool isCharging = false;
        bool isActive = true;
        Coroutine arrowSystemCooldown;


        private void Start()
        {
            targetSystem = GetComponent<TargetSystem>();
            movementControl = GetComponent<MovementControl>();

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

        private void StartFire()
        {
            if (!isActive) return;
            if (targetSystem.currentTarget == null || !targetSystem.currentTarget.isAvailable) return;

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

                CheckArrowRelease();
                
            }
            
            isCharging = false;
            DOTween.Kill(0);
            SetChargeAmount(0);
        }

        private void CheckArrowRelease()
        {
            DisableSystemForPeriod();

            if (HalfTime() || FullTime()){
                ShootCorrectArrow();
            }
            else{
                ShootWrongArrow();
            }
        }

        private bool HalfTime() => chargeAmount >= .5f - assistTimePercent && chargeAmount <= .5f + assistTimePercent;
        private bool FullTime() => chargeAmount >= 1 - assistTimePercent;

        private void ShootWrongArrow()
        {
            lockedTarget = targetSystem.currentTarget;
            wrongArrowParticle.transform.position = transform.position + Vector3.up * 0.7f;
            wrongArrowParticle.transform.LookAt(lockedTarget.transform.position);
            wrongArrowParticle.Play();
            print($"wrong arrow");
        }

        private void ShootCorrectArrow()
        {
            lockedTarget = targetSystem.currentTarget;
            correctArrowParticle.transform.position = lockedTarget.transform.position;
            var shape = correctArrowParticle.shape;
            shape.position = correctArrowParticle.transform.InverseTransformPoint(transform.position);
            correctArrowParticle.Play();
            
            print($"correct arrow");
        }

        private void DisableSystemForPeriod()
        {
            isActive = false;
            if (arrowSystemCooldown != null)
                StopCoroutine(arrowSystemCooldown);
            arrowSystemCooldown = StartCoroutine(SystemCooldown());

            IEnumerator SystemCooldown()
            {
                yield return new WaitForSeconds(systemCooldownTime);
                isActive = true;
            }
        }
    }
}