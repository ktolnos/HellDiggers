using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
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
        var left = Level.I.grid.GetCellCenterWorld(new Vector3Int(-Level.I.width / 2, 0, 0)).x;
        var right = Level.I.grid.GetCellCenterWorld(new Vector3Int(Level.I.width / 2 - 1, 0, 0)).x;
        var leftPos = left + halfViewport + 1.6667f;
        var rightPos = right - halfViewport + 1.6667f;
        var posX = Mathf.Clamp(transform.position.x, leftPos, rightPos);
        if (leftPos > rightPos)
        {
            posX = (leftPos + rightPos) / 2f;
        }

        transform.position = new Vector3(posX, transform.position.y, transform.position.z);
    }
}