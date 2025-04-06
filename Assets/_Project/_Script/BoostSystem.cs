using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Pathless_Recreation
{
    public class BoostSystem : MonoBehaviour
    {
        public float boostAmount;

        [Header("Boost Change Rate")] 
        [SerializeField] float boostGainAmount = 0.25f;
        [FormerlySerializedAs("boostDrainAmount")] [SerializeField] float boostDrainSpeed = 0.1f;

        [Header("Boost Visual")] 
        [SerializeField] Renderer boostMesh;
        [SerializeField] Slider boostSlider;

        private bool hasBoostAvailable;
        
        ArrowSystem arrowSystem;
        MovementControl movementControl;

        private void Awake()
        {
            arrowSystem = GetComponent<ArrowSystem>();
            movementControl = GetComponent<MovementControl>();
            
            boostSlider.value = boostAmount;
            VisualSetup();
        }

        #region Event Sub/UnSub

        private void OnEnable()
        {
            arrowSystem.OnTargetHit += IncreaseBoostAmount;
            movementControl.OnBoostStart += ActivateBoostEffect;
        }
        
        private void OnDisable()
        {
            arrowSystem.OnTargetHit -= IncreaseBoostAmount;
            movementControl.OnBoostStart -= ActivateBoostEffect;
        }

        #endregion

        private void Update()
        {
            if(movementControl.isRunning)
                boostAmount = boostAmount - boostDrainSpeed * Time.deltaTime;

            if (boostAmount <= 0 && hasBoostAvailable){
                hasBoostAvailable = false;
                movementControl.isRunning = false;
            }
            else{
                hasBoostAvailable = true;
            }
            
            boostSlider.value = boostAmount;
        }

        private void VisualSetup()
        {
            boostMesh.material.SetFloat("_Opacity", 0);
        }

        public void ActivateBoostEffect()
        {
            boostMesh.material.SetFloat("_TileAmount", 0);
            boostMesh.material.DOFloat(1, "_Opacity", .1f);
            boostMesh.material.DOFloat(1, "_TileAmount", .4f);
            boostMesh.material.DOFloat(0, "_Opacity", .20f).SetDelay(.2f);
        }

        private void IncreaseBoostAmount()
        {
            float total = boostAmount + boostGainAmount;
            boostAmount = Mathf.Clamp01(total);
        }
    }
}