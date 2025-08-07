using System;
using UnityEngine;
using IngameDebugConsole;
using UnityEngine.Serialization;

public class CustomCommands : MonoBehaviour
{
    public GameObject fpsCounter;
    private static CustomCommands I;

    private void Start()
    {
        I = this;
    }

    [ConsoleMethod( "free", "Toggles free purchase of items" )]
    public static bool Free()
    {
        GM.I.isFree = !GM.I.isFree;
        return GM.I.isFree;
    }
    
    [ConsoleMethod( "free", "Toggles free purchase of items" )]
    public static bool Free(bool free)
    {
        GM.I.isFree = free;
        return GM.I.isFree;
    }
    
    
    [ConsoleMethod( "money", "Toggles free purchase of items" )]
    public static string Money(int money)
    {
        GM.I.money = money;
        return "Set money to " + money;
    }
    
    
    [ConsoleMethod( "fps", "Toggles FPS counter" )]
    public static string ToggleFPSCounter()
    {
        I.fpsCounter.SetActive(!I.fpsCounter.activeSelf);
        return "OK";
    }

}