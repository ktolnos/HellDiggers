using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageNumber : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float floatSpeed = 1f;
    private Vector3 moveDirection;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        moveDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f).normalized;
    }

    private void Update()
    {
        transform.position += moveDirection * floatSpeed * Time.deltaTime;
    }
        
}