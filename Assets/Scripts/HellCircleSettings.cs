using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

public class HellCircleSettings: MonoBehaviour
{
    public string circleName;
    public Color color;
    public TileData[] tileData;
    public float noiseThreshold = -0.2f;
    public float noiseFrequency = 0.15f;
    public GameObject boss;

    
    [Serializable]
    public class TileData
    {
        public string name;
        public TileBase tile;
        public TileBase[] variants;
        public float maxHp = 1f;
        public float spawnChance = 1f;
        public float surfaceSpawnChanceMult = 1f;
        public bool randomRotation = true;
        
        public GameObject drop;
        public float dropChance = 0f;
        public float contactDamage = 0f;
        public bool isSlippery = false;
        public bool isMud = false;
        public float outForce = 0f;
        public float lootLifetime = 10f;
        public bool allowLava = false;
    }
}