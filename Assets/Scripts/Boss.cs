using System;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    private static readonly int Progress = Shader.PropertyToID("_Progress");

    private Image healthBar;
    private Health health;
    private Material healthMaterial;
    private void Start()
    {
        healthBar = HUD.I.bossHealthBar;
        healthMaterial =healthBar.material;
        health = GetComponent<Health>();
    }

    private void Update()
    {
        healthBar.gameObject.SetActive(Level.I.isInBossRoom && health.currentHealth > 0);
        var healthPercent = health.currentHealth / health.maxHealth;
        healthMaterial.SetFloat(Progress, healthPercent);
        healthBar.SetMaterialDirty();
    }

    private void OnDestroy()
    {
        healthBar.gameObject.SetActive(false);
    }
}
