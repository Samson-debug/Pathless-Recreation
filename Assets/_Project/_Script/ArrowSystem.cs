using System;
using Pathless_Recreation;
using UnityEngine;
using UnityEngine.Rendering;

public class ArrowSystem : MonoBehaviour
{
    public ParticleSystem wrongArrowParticle;
    public ParticleSystem correctArrowParticle;
    
    TargetSystem targetSystem;

    private void Start()
    {
        targetSystem = GetComponent<TargetSystem>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)){
            ShootWrongArrow();
        }
        else if (Input.GetMouseButtonDown(1)){
            ShootCorrectArrow();
        }
    }

    private void ShootWrongArrow()
    {
        wrongArrowParticle.transform.LookAt(targetSystem.currentTarget.transform.position);
        wrongArrowParticle.Play();
    }

    private void ShootCorrectArrow()
    {
        correctArrowParticle.transform.position = targetSystem.currentTarget.transform.position;
        var shape = correctArrowParticle.shape;
        shape.position = correctArrowParticle.transform.InverseTransformPoint(transform.position);
        correctArrowParticle.Play();
    }
}
