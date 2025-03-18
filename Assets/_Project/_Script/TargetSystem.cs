using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class TargetSystem : MonoBehaviour
{
    public static TargetSystem instance;
    public static List<ArrowTarget> targets = new List<ArrowTarget>();
    public static List<ArrowTarget> reachableTargets = new List<ArrowTarget>();

    [Header("Visual Elements")]
    public RectTransform targetFocusSprite;
    public float rectSizeMultiplier = 2f;
    
    [Header("Parameters")]
    //Weight values that determine what distance (screen/player) gets prioritized
    [SerializeField] float screenDistanceWeight = 1;
    [SerializeField] float positionDistanceWeight = 8;
    public float maxReachDistance = 70;
    
    ArrowTarget currentTarget, storedTarget;
    Camera cam;
    private void Awake()
    {
        if(instance == null)
            instance = this;
        
        cam = Camera.main;
    }

    private void FixedUpdate()
    {
        SetTargetFocus();
        CheckTargetFocusChange();
        
        if(currentTarget != null)
        {
            targetFocusSprite.position = WorldToScreenPoint(currentTarget.transform.position);
            float distanceFromTarget = Vector3.Distance(currentTarget.transform.position, transform.position);
            targetFocusSprite.sizeDelta = new Vector2(
                Mathf.Clamp(115 - (distanceFromTarget / rectSizeMultiplier),50,200),
                Mathf.Clamp(115 - (distanceFromTarget / rectSizeMultiplier),50,200)
            );
        }
    }

    private void SetTargetFocus()
    {
        if (storedTarget != currentTarget)
        {
            targetFocusSprite.gameObject.SetActive( currentTarget != null);
            storedTarget = currentTarget;
            targetFocusSprite.DOComplete();
            targetFocusSprite.DOScale(4, 0.5f).From();
        }
    }
    private void CheckTargetFocusChange()
    { 
        int targetIndex = TargetIndex();
        currentTarget = targetIndex < 0 ? null : reachableTargets[targetIndex];
        
        if (currentTarget != null) print($"Current target: {currentTarget.gameObject.name} && with index: {targetIndex}");
        else print($"No current target  && index of {targetIndex}");
    }

    private int TargetIndex()
    {
        float[] distances = new float[reachableTargets.Count];

        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = DistanceFromCenterLine(reachableTargets[i].transform.position, reachableTargets[i].gameObject);
            print($"{reachableTargets[i].gameObject.name} has distance of {distances[i]}");
            //Debug.Log($"Obj : {targets[i].gameObject.name} || Distance: {DistanceFromCenterLine(targets[i].transform.position, targets[i].gameObject)}");
            
        }
        
        float minDistance = distances.Where(n => n > 0).DefaultIfEmpty(-1).Min(); //Find the samllest positive value if no value is fount return -1
        
        int targetIndex = minDistance < 0 ? -1 : Array.IndexOf(distances, minDistance); //if value is not part of array it will return -1
        //Debug.Log($"TargetIndex Obj : {targets[targetIndex].gameObject.name} || target index : {targetIndex}");
        return targetIndex;
    }

    private float DistanceFromCenterLine(Vector3 of, GameObject target)
    {
        float screenWidth = Screen.width;
        float centerX = screenWidth / 2f;

        Vector3 screenPos = WorldToScreenPoint(of);
        
        print($"{target.name} at {screenPos}");
        
        // Check if behind camera
        if (screenPos.z < 0)
            return -1;
        
        //Check if obj is outside frustum
        if((screenPos.x < 0 || screenPos.x > screenWidth)
           || (screenPos.y < 0 || screenPos.y > Screen.height))
            return -1;
        
        float distance = Mathf.Abs(screenPos.x - centerX) * screenDistanceWeight + 
                           Vector3.Distance(cam.transform.position, of) * positionDistanceWeight;

        return distance;
    }

    private Vector3 WorldToScreenPoint(Vector3 worldPos) => cam.WorldToScreenPoint(worldPos);
}