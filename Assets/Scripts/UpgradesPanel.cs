using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradesPanel: MonoBehaviour, IDragHandler
{
    public static UpgradesPanel I;
    private RectTransform rectTransform;
    private Vector2 originalLocalPointerPosition;
    private Vector2 originalPanelLocalPosition;
    public float centerAnimDuration = 0.3f;
    
    private float centerAnimTime;
    private bool isCentering;
    private Vector2 targetPosition;

    private void Awake()
    {
        I = this;
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData data)
    {
        rectTransform.anchoredPosition += data.delta / UpgradesController.I.upgradeUI.scaleFactor;
        isCentering = false;
    }

    private void Update()
    {
        if (!isCentering) return;
        centerAnimTime += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(centerAnimTime / centerAnimDuration);
        t = DOVirtual.EasedValue(0, 1, t, Ease.OutCubic);
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, t);
    }

    public void CenterOnSkill(Skill skill)
    {
        isCentering = true;
        targetPosition = -skill.rectTransform.anchoredPosition;
        centerAnimTime = 0f;
        targetPosition = -skill.rectTransform.anchoredPosition;
    }
}