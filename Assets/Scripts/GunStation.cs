using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GunStation : MonoBehaviour, IInteractionHandler
{
    public static HashSet<string> purchasedGuns = new();
    private Interactable interactable;
    [FormerlySerializedAs("gun")] public Gun gunPrefab;
    private Gun gun;
    private Vector3 gunOffset = new Vector3(0, 1, 0);
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI gunNameText;
    private void Start()
    {
        gun = Instantiate(gunPrefab, transform.position + gunOffset, Quaternion.identity, transform);
        gun.gunStation = this;
        interactable = GetComponent<Interactable>();
        if (gun.id == Player.I.currentGunId || gun.id == Player.I.secondaryGunId)
        {
            SetGun();
        }

        if (gun.price == 0)
        {
            purchasedGuns.Add(gun.id);
        }
        gun.gunName.StringChanged += OnGunNameChanged;
        OnGunNameChanged(gun.gunName.GetLocalizedString());
    }
    
    private void OnGunNameChanged(string localizedString)
    {
        gunNameText.text = localizedString;
    }
    
    public void Interact()
    {
        if (GM.I.money >= gun.price)
        {
            GM.I.money -= -gun.price;
            purchasedGuns.Add(gun.id);
        }
        if (purchasedGuns.Contains(gun.id))
        {
            SetGun();
        }
    }
    
    private void Update()
    {
        if (gun.transform.parent == transform)
        {
            gun.transform.localPosition = gunOffset + new Vector3(0, 0.2f * Mathf.Sin(Time.time * 2f), 0);
            if (purchasedGuns.Contains(gun.id))
            {
                priceText.text = "";
            }
            else
            {
                priceText.text = gun.price.ToString();
            }
            priceText.color = GM.I.money >= gun.price ? Color.white : Color.red;
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
        gun.reloadIndicator = null;
    }
}
