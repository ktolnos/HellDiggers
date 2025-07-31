using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController: MonoBehaviour
{
    public static CameraController I;

    private Camera cam;
    public float scopeBias = 0.5f; // Bias for the camera position when scoped

    private void Awake()
    {
        I = this;
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        transform.position = Player.I.transform.position * (1f - scopeBias) + cam.ScreenToWorldPoint(Mouse.current.position.value) * scopeBias;
        var halfViewport = (cam.orthographicSize * cam.aspect);
        var worldSize = Level.I.width / 2f;
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -worldSize + halfViewport + 1f, worldSize - halfViewport + 1f),
            transform.position.y,
            transform.position.z
        );
    }
}