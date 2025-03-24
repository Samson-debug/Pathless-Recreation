using System;
using UnityEngine;

namespace Pathless_Recreation
{
    public class ArrowTarget : MonoBehaviour
    {
        private Transform player;

        bool isAvailable = true;
        bool isReachable = false;
        Vector3 targetDirection;

        private void Awake()
        {
            player = FindAnyObjectByType<MovementControl>().transform;
        }

        private void Start()
        {
            OnBecameVisible();
        }

        private void Update()
        {
            var playerToTarget = transform.position - player.position;
            bool isBehindPlayer = Vector3.Dot(Camera.main.transform.forward, playerToTarget) < 0;

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
                if (TargetSystem.reachableTargets.Contains(this))
                    TargetSystem.reachableTargets.Remove(this);
            }
        }

        private void OnBecameVisible()
        {
            if (!TargetSystem.targets.Contains(this))
                TargetSystem.targets.Add(this);
        }

        private void FixedUpdate()
        {
            if(isAvailable)
                transform.LookAt(player.position);
        }
    }
}