using System;
using UnityEngine;
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
        
        public float explosionRadius = 0f;
        public float explosionDamage = 0f;
        public float explosionEnemyDamage = 0f;
        public float explosionDelay = 1f;
        public float explosionFPS = 10f;
        
        public TileBase[] damagedTiles;
        public GameObject drop;
        public float dropChance = 0f;
    }
}