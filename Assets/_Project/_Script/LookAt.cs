using Pathless_Recreation;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    Transform player;
    ArrowTarget arrowTarget;

    private void Awake()
    {
        player = FindFirstObjectByType<MovementControl>().transform;
        arrowTarget = GetComponentInChildren<ArrowTarget>();
    }

    private void FixedUpdate()
    {
        if(arrowTarget.isAvailable)
            transform.LookAt(player.position);
    }
}