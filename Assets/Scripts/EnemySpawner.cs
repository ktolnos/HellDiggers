using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;


public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner I;
    public List<EnemyData> enemies;
    public GameObject portal;
    public int spawnRadius;
    public int enemyAmount;
    public float revertedHardometr;
    public float spawnRate;
    public float portalDelay;
    public float randomSpawnDelayMax;
    private float timeOfLastSpawn;
    
    private void Awake()
    {
        I = this;
    }

    void Start()
    {
        timeOfLastSpawn = Time.time;
    }

    void Update()
    {        
        if (Time.time - timeOfLastSpawn > spawnRate/((Time.time - Level.I.timeOfLevelStart)/revertedHardometr))
        {
            Spawner(randomSpawnDelayMax, enemyAmount, portalDelay, spawnRadius, false);
            timeOfLastSpawn = Time.time;
        }
    }

    public void Spawner(float spawnDelay, int amount, float portalDelay, int spawnRadius, bool startSpawn)
    {
        for (int i = 0; i < amount; i++)
        {
            StartCoroutine(Spawn(spawnDelay, portalDelay, spawnRadius, startSpawn));
        }
    }

    public IEnumerator Spawn(float spawnDelay, float portalDelay, int spawnRadius, bool startSpawn)
    {
        bool foundLocation = false;
        Vector3Int location = Vector3Int.zero;
        int count = 0;
        
        while (!foundLocation && count < 100)
        {
            Vector3Int tryPosition = Vector3Int.one;
            if (startSpawn)
            {
                tryPosition = new Vector3Int(UnityEngine.Random.Range(-spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).x, spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).x),
                              0, 0);  
            }
            else
            {
                tryPosition = new Vector3Int(UnityEngine.Random.Range(-spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).x, spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).x),
                    UnityEngine.Random.Range(-spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).y, spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).y), 0); 
            }
            
            if (!Level.I.tilemap.HasTile(tryPosition))
            {
                location = tryPosition;
                foundLocation = true;
            }

            count ++;
        }
        if (foundLocation)
        {
            var totalProbability = enemies.Sum(x => x.chanceToSpawnOnCircle[Level.I.currentCircleIndex]);
            var randomValue = UnityEngine.Random.value * totalProbability;
            GameObject enemy = null;
            for (int i = 0; i < enemies.Count; i++)
            {
                var data = enemies[i];
                if (randomValue < data.chanceToSpawnOnCircle[Level.I.currentCircleIndex])
                {
                    enemy = data.gameObject;
                    break;
                }
                randomValue -= data.chanceToSpawnOnCircle[Level.I.currentCircleIndex];
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, spawnDelay));
            Destroy(Instantiate(portal, Level.I.tilemap.CellToWorld(location), Quaternion.identity, Level.I.spawnedObjectsParent), portalDelay);
            yield return new WaitForSeconds(portalDelay);
            if (enemy != null)
            {
                Instantiate(enemy, Level.I.tilemap.CellToWorld(location), Quaternion.identity, Level.I.spawnedObjectsParent);
            }
        }
    }
    
    [Serializable]
    public class EnemyData
    {
        public GameObject gameObject;
        public List<float> chanceToSpawnOnCircle = new List<float>(9); 
    }
}
