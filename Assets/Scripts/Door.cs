using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject top;
    public GameObject bottom;
    private bool isOpen = false;
    public float openingTime = 0.5f;
    public Vector3 targetPositionTop;
    public Vector3 targetPositionBottom;
    private Vector3 startTop;
    private Vector3 startBottom;
    private float t;

    private void Start()
    {
        startTop = top.transform.localPosition;
        startBottom = bottom.transform.localPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        isOpen = true;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        isOpen = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        isOpen = true;
    }

    private void Update()
    {
        var increment = Time.deltaTime / openingTime;
        t += isOpen ? increment : -increment;
        t = Mathf.Clamp01(t);
        top.transform.localPosition = Vector3.Lerp(startTop, targetPositionTop, t);
        bottom.transform.localPosition = Vector3.Lerp(startBottom, targetPositionBottom, t);
    }
}
