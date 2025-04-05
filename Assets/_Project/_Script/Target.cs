using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Pathless_Recreation
{
    public class ArrowTarget : MonoBehaviour
    {
        Transform player;
        MovementControl playerController;

        [Header("Availability Setting")]
        public bool isAvailable = true;
        bool isReachable = false;

        [Header("Controllers")] 
        [SerializeField] Renderer detector;
        [SerializeField] Renderer flameRenderer;
        [SerializeField] Renderer imageRenderer;
        [SerializeField] ParticleSystem hitParticles;
        [SerializeField] Collider collisionDetector;
        [SerializeField] GameObject arrowObj;
        
        Vector3 targetDirection;
        Color originalColor;

        private void Awake()
        {
            playerController = FindFirstObjectByType<MovementControl>();
            player = playerController.transform;
            originalColor = imageRenderer.material.color;
        }

        private void Start()
        {
            OnBecameVisible();
        }

        private void Update()
        {
            var playerToTarget = transform.position - player.position;
            bool isBehindPlayer = Vector3.Dot(Camera.main.transform.forward, playerToTarget) < 0 && playerController.isRunning;

            if (playerToTarget.magnitude < TargetSystem.instance.maxReachDistance && !isBehindPlayer && !isReachable)
            {
                //Debug.Log($"{gameObject.name} is reachable");
                isReachable = true;
                if (TargetSystem.targets.Contains(this))
                    TargetSystem.reachableTargets.Add(this);
            }

            if ((playerToTarget.magnitude > TargetSystem.instance.maxReachDistance || isBehindPlayer) && isReachable)
            {
                //Debug.Log($"{gameObject.name} is out of reach");
                isReachable = false;
                TargetSystem.targets.Remove(this);
                if (TargetSystem.instance.currentTarget == this)
                    TargetSystem.instance.StopTargetFocus();
            }
        }

        private void OnBecameVisible()
        {
            if (!TargetSystem.targets.Contains(this) && isAvailable){
                TargetSystem.targets.Add(this);
                
                if(isReachable)
                    TargetSystem.reachableTargets.Add(this);
            }
        }

        private void OnBecameInvisible()
        {
            TargetSystem.targets.Remove(this);
            TargetSystem.reachableTargets.Remove(this);

            if (TargetSystem.instance.currentTarget == this)
            {
                TargetSystem.instance.StopTargetFocus();
            }
        }

        public void DisableTarget(Vector3 hitObjVelocity)
        {
            isAvailable = false;
            collisionDetector.enabled = false;
            arrowObj.SetActive(true);
            
            imageRenderer.transform.DOPunchPosition(-Vector3.forward * 7, .5f, 2, 1, false);
            flameRenderer.enabled = false;
            hitParticles.Play();
            imageRenderer.material.color = Color.black;
            
            if (TargetSystem.targets.Contains(this))
                TargetSystem.targets.Remove(this);
            if (TargetSystem.reachableTargets.Contains(this))
                TargetSystem.reachableTargets.Remove(this);

            StartCoroutine(ReactivateCoroutine());

            IEnumerator ReactivateCoroutine()
            {
                yield return new WaitForSeconds(TargetSystem.instance.disableCooldownTime);
                
                isAvailable = true;
                arrowObj.SetActive(false);
                flameRenderer.enabled = true;
                imageRenderer.material.color = originalColor;
                imageRenderer.enabled = true;
                collisionDetector.enabled = true;

                if (detector.isVisible && !TargetSystem.targets.Contains(this)){
                    TargetSystem.targets.Add(this);
                    if(isReachable)
                        TargetSystem.reachableTargets.Add(this);
                }
            }
        }
        
    }
}