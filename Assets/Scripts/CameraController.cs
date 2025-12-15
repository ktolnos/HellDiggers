using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController I;

    private Camera cam;
    public float scopeBias = 0.5f; // Bias for the camera position when scoped
    private Vector3 offset;
    public float speed = 1f;

    private void Awake()
    {
        I = this;
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        var mousePosOffset =  VirtualMouseController.I.mousePosition - new Vector2(Screen.width / 2f, Screen.height / 2f);
        mousePosOffset /= Screen.height / 2f; // Normalize to viewport coordinates
        offset = Vector3.MoveTowards(offset, (Vector3)mousePosOffset * scopeBias, Time.deltaTime * speed);
        transform.position = Player.I.transform.position + offset;
        var halfViewport = (cam.orthographicSize * cam.aspect);
        var left = Level.I.grid.GetCellCenterWorld(new Vector3Int(-Level.I.width / 2, 0, 0)).x;
        var right = Level.I.grid.GetCellCenterWorld(new Vector3Int(Level.I.width / 2 - 1, 0, 0)).x;
        var leftPos = left + halfViewport;
        var rightPos = right - halfViewport;
        var posX = Mathf.Clamp(transform.position.x, leftPos, rightPos);
        if (leftPos > rightPos)
        {
            posX = (leftPos + rightPos) / 2f;
        }

        transform.position = new Vector3(posX, transform.position.y, transform.position.z);
    }
    
    public static bool IsObjectVisible(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(I.cam);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}