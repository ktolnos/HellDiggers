using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageNumber : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float floatSpeed = 1f;
    private Vector3 moveDirection;
    private float startTime;
    public float duration = 0.7f;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        moveDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f).normalized;
        startTime = Time.time;
    }

    private void Update()
    {
        transform.position += moveDirection * floatSpeed * Time.deltaTime;
        var t = (Time.time - startTime) / duration;
        text.color = Color.Lerp(Color.white, Color.clear, t);
    }
        
}