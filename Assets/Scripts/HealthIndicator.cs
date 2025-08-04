using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    public Image healthBar;
    private Health _health;

    private void Start()
    {
        _health = GetComponent<Health>();
    }

    private void Update()
    {
        healthBar.fillAmount = _health.currentHealth / _health.maxHealth;
    }
}
