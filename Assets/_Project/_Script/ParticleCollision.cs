using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Pathless_Recreation
{
    public class ParticleCollision : MonoBehaviour
    {
        public bool isEnergy;
        public List<Renderer> characterRenderers;
        public ParticleSystem sprinkleOnWalkPart;

        ParticleSystem part;
        ArrowSystem arrowSystem;
        float amount;
        
        List<ParticleCollisionEvent> collisionEvents = new();

        private void Start()
        {
            part = GetComponent<ParticleSystem>();
            arrowSystem = FindFirstObjectByType<ArrowSystem>();
        }

        private void Update()
        {
            if(!isEnergy) return;
            
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[part.particleCount];
            part.GetParticles(particles);

            for (int i = 0; i < particles.Length; i++){
                ParticleSystem.Particle particle = particles[i];
                if (particle.remainingLifetime > 0 && Vector3.Distance(particle.position, transform.position) < 1f){
                    particle.remainingLifetime = 0f;
                    particle.startSize = 0f;

                    if (amount < 1f){
                        amount++;

                        foreach (Renderer renderer in characterRenderers){
                            renderer.material.DOFloat(1f, "_Alpha", 0.2f).OnComplete(() => OnComplete(renderer));
                        }
                    }
                }
            }
            
        }

        private void OnComplete(Renderer renderer)
        {
            renderer.material.DOFloat(0, "_Alpha", 0.3f).OnComplete(() => amount = 0f);
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