using UnityEngine;
using UnityEngine.Serialization;

public class GunStation : MonoBehaviour, IInteractionHandler
{
    private Interactable interactable;
    [FormerlySerializedAs("gun")] public Gun gunPrefab;
    private Gun gun;
    private Vector3 gunOffset = new Vector3(0, 1, 0);
    private void Start()
    {
        gun = Instantiate(gunPrefab, transform.position + gunOffset, Quaternion.identity, transform);
        gun.gunStation = this;
        interactable = GetComponent<Interactable>();
        if (gun.id == Player.I.currentGunId)
        {
            SetGun();
        } 
    }
    
    public void Interact()
    {
        SetGun();
    }
    
    private void Update()
    {
        if (gun.transform.parent == transform)
        {
            gun.transform.localPosition = gunOffset + new Vector3(0, 0.2f * Mathf.Sin(Time.time * 2f), 0);
        }
    }

    private void SetGun()
    {
        Player.I.SetGun(gun); 
        interactable.IsInteractable = false;
    }

    public void ResetGun()
    {
        gun.transform.parent = transform;
        gun.transform.localPosition = gunOffset;
        gun.transform.localRotation = Quaternion.identity;
        interactable.IsInteractable = true;
        gun.animator.spriteRenderer.flipY = false;
    }
}
