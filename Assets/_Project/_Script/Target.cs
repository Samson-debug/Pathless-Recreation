using System;
using UnityEngine;

public class ArrowTarget : MonoBehaviour
{
    public Transform player;

    bool isReachable = false;
    Vector3 targetDirection;
    private void Awake()
    {
        if(player == null)
            Debug.LogError("Player is null");
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
            if(TargetSystem.targets.Contains(this))
                TargetSystem.reachableTargets.Add(this);
        }
        
        if ((playerToTarget.magnitude > TargetSystem.instance.maxReachDistance || isBehindPlayer) && isReachable)
        {
            //Debug.Log($"{gameObject.name} is out of reach");
            isReachable = false;
            if(TargetSystem.reachableTargets.Contains(this))
                TargetSystem.reachableTargets.Remove(this);
        }
    }

    private void OnBecameVisible()
    {
        if(!TargetSystem.targets.Contains(this))
            TargetSystem.targets.Add(this);
    }
    private void FixedUpdate()
    {
        transform.LookAt(player.position);
    }
}