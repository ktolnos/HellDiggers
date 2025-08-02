using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD: MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moneyText;
    
    public Image healthImage;
    public Material healthMaterial;
    
    private static readonly int Progress = Shader.PropertyToID("_Progress");

    private void Update()
    {
        moneyText.text = Player.I.money.ToString();
        if (healthText != null)
        {
            healthText.text = Mathf.CeilToInt(Player.I.health.currentHealth).ToString();
        }
        var healthPercent = Player.I.health.currentHealth / Player.I.health.maxHealth;
        healthMaterial.SetFloat(Progress, healthPercent);
        healthImage.rectTransform.sizeDelta = new Vector2(Player.I.health.maxHealth * 1f, healthImage.rectTransform.sizeDelta.y);
        healthImage.SetMaterialDirty();
    }
}