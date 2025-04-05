using System.Collections.Generic;
using UnityEngine;

namespace Pathless_Recreation
{
    public class ParticleCollision : MonoBehaviour
    {
        public bool isEnergy;

        ParticleSystem part;
        ArrowSystem arrowSystem;
        
        List<ParticleCollisionEvent> collisionEvents = new();

        private void Start()
        {
            part = GetComponent<ParticleSystem>();
            arrowSystem = FindFirstObjectByType<ArrowSystem>();
        }

        private void OnParticleCollision(GameObject other)
        {
            if(other.layer == 7) return; //if ground
            if(isEnergy) return;

            part.GetCollisionEvents(other.gameObject, collisionEvents);
            arrowSystem.TargetHit(collisionEvents[0].velocity);
        }
    }
}