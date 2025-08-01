using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;


public class EnemySpawner : MonoBehaviour
{
    public List<EnemyData> enemies;
    public int spawnRadius;
    public float spawnRate;
    public float randomSpawnDelayMax;
    private float totalProbability;
    private float timeOfLastSpawn; 
    void Start()
    {
        timeOfLastSpawn = Time.time;
    }

    void Update()
    {        
        if (Time.time - timeOfLastSpawn > spawnRate)
        {
            Spawner();
            timeOfLastSpawn = Time.time;
        }
    }

    void Spawner()
    {
        bool foundLocation = false;
        Vector3Int location = Vector3Int.zero;
        int count = 0;
        while (!foundLocation && count < 100)
        {
            Vector3Int tryPosition = new Vector3Int(UnityEngine.Random.Range(-spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).x, spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).x),
                UnityEngine.Random.Range(-spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).y, spawnRadius+Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position).y), 0);
            if (!Level.I.tilemap.HasTile(tryPosition))
            {
                location = tryPosition;
                foundLocation = true;
                Debug.Log(location);
            }

            count ++;
        }

        StartCoroutine(Spawn(location));

        
    }

    public IEnumerator Spawn(Vector3Int location)
    {
        totalProbability = enemies.Sum(x => x.chanceToSpawnOnCircle[Level.I.currentCircleIndex]);
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
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, randomSpawnDelayMax));
        if (enemy != null)
        {
            Instantiate(enemy, Level.I.tilemap.CellToWorld(location), Quaternion.identity);
        }
    }
    
    [Serializable]
    public class EnemyData
    {
        public GameObject gameObject;
        public List<float> chanceToSpawnOnCircle = new List<float>(9); 
    }
}
