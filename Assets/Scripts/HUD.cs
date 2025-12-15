using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD: MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moneyText;
    
    public TextMeshProUGUI copperText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI emeraldText;
    public TextMeshProUGUI diamondText;
    
    public Image healthImage;
    public Material healthMaterial;
    
    public Image jetFuelIndicator;
    
    private static readonly int Progress = Shader.PropertyToID("_Progress");

    private void Update()
    {
        moneyText.text = GM.I.money.ToString();
        if (healthText != null)
        {
            healthText.text = Mathf.CeilToInt(Player.I.health.currentHealth).ToString();
        }
        var healthPercent = Player.I.health.currentHealth / Player.I.health.maxHealth;
        healthMaterial.SetFloat(Progress, healthPercent);
        healthImage.rectTransform.sizeDelta = new Vector2(Player.I.health.maxHealth * 1f, healthImage.rectTransform.sizeDelta.y);
        healthImage.SetMaterialDirty();
        
        jetFuelIndicator.gameObject.SetActive(Player.I.health.currentHealth > 0f);
        var stats = Player.I.stats;
        jetFuelIndicator.fillAmount =  Player.I.jetPackFuel / (Player.I.stats.jetPackFuel * Player.I.jetFuelMult);
        jetFuelIndicator.rectTransform.sizeDelta = new Vector2(jetFuelIndicator.rectTransform.sizeDelta.x,  stats.jetPackFuel * 150f);
        
        copperText.text = GM.I.resources.copper.ToString();
        ironText.text = GM.I.resources.iron.ToString();
        goldText.text = GM.I.resources.gold.ToString();
        emeraldText.text = GM.I.resources.emerald.ToString();
        diamondText.text = GM.I.resources.diamond.ToString();
        
        copperText.transform.parent.gameObject.SetActive(GM.I.resources.copper > 0);
        ironText.transform.parent.gameObject.SetActive(GM.I.resources.iron > 0);
        goldText.transform.parent.gameObject.SetActive(GM.I.resources.gold > 0);
        emeraldText.transform.parent.gameObject.SetActive(GM.I.resources.emerald > 0);
        diamondText.transform.parent.gameObject.SetActive(GM.I.resources.diamond > 0);  
    }
}