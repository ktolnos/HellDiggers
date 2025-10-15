using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseHelper: MonoBehaviour
{
    private InputAction pauseAction;
    public bool isPaused = false;
    public GameObject pausePanel;

    private void Start()
    {
        pauseAction = InputSystem.actions.FindAction("Pause");
    }

    private void Update()
    {
        if (pauseAction.WasPressedThisFrame() && !GM.IsUIOpen)
        {
            Pause(!isPaused);
        }
    }

    public void Pause(bool pause)
    {
        isPaused = pause;
        Time.timeScale = isPaused ? 0f : 1f;
        pausePanel.SetActive(isPaused);
    }
}