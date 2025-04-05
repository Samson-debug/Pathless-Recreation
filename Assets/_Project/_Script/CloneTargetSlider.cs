using System;
using DG.Tweening;
using Pathless_Recreation;
using UnityEngine;
using UnityEngine.UI;

public class CloneTargetSlider : MonoBehaviour
{
    [SerializeField] float effectDuration = 0.4f;
    [SerializeField] float scale = 1.6f;
    [SerializeField] Ease ease = Ease.OutSine;

    [SerializeField] Image fillImage;
    
    CanvasGroup canvasGroup;
    Slider slider;
    RectTransform rect;
    Transform referenceTransform;
    Transform player;
    Vector3 initialScale;
    
    private void Awake()
    {
        slider = GetComponent<Slider>();
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        player = FindFirstObjectByType<MovementControl>().transform;
        
        initialScale = rect.localScale;
        
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        canvasGroup.DOFade(0f, effectDuration);
        rect.DOScale(scale, effectDuration).SetEase(ease).OnComplete(() => OnComplete());
    }

    private void Update()
    {
        if(referenceTransform == null) return;
        
        Vector3 PlayerToRef = referenceTransform.position - player.position;
        bool isBehindPlayer = Vector3.Dot(PlayerToRef, Camera.main.transform.forward) < 0;
        
        transform.position = isBehindPlayer ? transform.position : Camera.main.WorldToScreenPoint(referenceTransform.position);
        
        print($"Clone slider fade {canvasGroup.alpha} & scale : {rect.localScale}");
    }

    public void SetUp(Transform referenceTransform, Vector2 sizeDelta, float chargeValue)
    {
        this.referenceTransform = referenceTransform;
        rect.sizeDelta = sizeDelta;
        slider.value = chargeValue;

        if (chargeValue == 0.5 || chargeValue == 1){
            fillImage.color = Color.red;
        }
        
        print("Clone SLider Setting up");
    }

    private void OnComplete()
    {
        print("Clone effect Complete");
        gameObject.SetActive(false);
    }
    
    private void OnDisable()
    {
        canvasGroup.alpha = 1f;
        fillImage.color = Color.white;
        rect.localScale = initialScale;
        referenceTransform = null;

        print("Clone Slider Disabled");
    }
}
