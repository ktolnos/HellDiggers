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
        var mousePosOffset = Mouse.current.position.ReadValue() - new Vector2(Screen.width / 2f, Screen.height / 2f);
        mousePosOffset /= Screen.height / 2f; // Normalize to viewport coordinates
        transform.position = Player.I.transform.position + (Vector3)mousePosOffset * scopeBias;
        var halfViewport = (cam.orthographicSize * cam.aspect);
        var left = Level.I.tilemap.GetCellCenterWorld(new Vector3Int(-Level.I.width / 2, 0, 0)).x;
        var right = Level.I.tilemap.GetCellCenterWorld(new Vector3Int(Level.I.width / 2 - 1, 0, 0)).x;
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, left + halfViewport + 1.6667f, right - halfViewport + 1.6667f),
            transform.position.y,
            transform.position.z
        );
    }
}