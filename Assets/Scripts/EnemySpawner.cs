using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;


public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner I;
    public List<EnemyData> enemies;
    public GameObject portal;
    public int spawnRadius;
    public int enemyAmount;
    [FormerlySerializedAs("spawnRate")] public float hardometer;
    public float portalDelay;
    public float randomSpawnDelayMax;
    public int cellSize;
    private float timeOfLastSpawn;
    public List<int> numPrespawnedEnemies = new();
    public float timeHardnessMult;

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
        if (Player.I.health.currentHealth <= 0)
        {
            return;
        }

        var timeSinceStart = Time.time - Level.I.timeOfLevelStart;
        if (Level.I.isLevelTransition)
        {
            timeOfLastSpawn = Time.time;
        }

        if (Time.time - timeOfLastSpawn > (1f / hardometer) / (Mathf.Pow(timeSinceStart, 2f)))
        {
            Spawner(false);
            timeOfLastSpawn = Time.time;
        }
    }

    public void Spawner(bool startSpawn)
    {
        var amount = enemyAmount;
        if (startSpawn)
        {
            amount = numPrespawnedEnemies[Level.I.currentCircleIndex];
        }

        for (int i = 0; i < amount; i++)
        {
            StartCoroutine(Spawn(startSpawn));
        }
    }

    public IEnumerator Spawn(bool startSpawn)
    {
        bool foundLocation = false;
        Vector3Int location = Vector3Int.zero;
        int count = 0;

        while (!foundLocation && count < 100)
        {
            Vector3Int tryPosition = Vector3Int.one;
            if (startSpawn)
            {
                var left = -Level.I.width / 2f + 5f;
                var right = Level.I.width / 2f - 5f;
                var height = -Level.I.height;
                tryPosition = new Vector3Int(Mathf.RoundToInt(UnityEngine.Random.Range(left, right)),
                    Mathf.RoundToInt(UnityEngine.Random.Range(0, height)), 0);
            }
            else
            {
                var playerPos = Level.I.tilemap.WorldToCell(Player.I.gameObject.transform.position);
                tryPosition = new Vector3Int(
                    UnityEngine.Random.Range(
                        -spawnRadius + playerPos.x,
                        spawnRadius + playerPos.x),
                    UnityEngine.Random.Range(
                        -spawnRadius + playerPos.y,
                        spawnRadius * 2 + playerPos.y), 0);
            }

            if (!Level.I.tilemap.HasTile(tryPosition))
            {
                location = tryPosition;
                foundLocation = true;
            }

            count++;
        }

        if (foundLocation)
        {
            var enemy = startSpawn ? SelectStartSpawnEnemy() : SelectPortalEnemy();
            if (enemy != null)
            {
                if (!startSpawn)
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0f, randomSpawnDelayMax));
                    Destroy(
                        Instantiate(portal, Level.I.tilemap.GetCellCenterWorld(location), Quaternion.identity,
                            Level.I.spawnedObjectsParent), portalDelay);
                    yield return new WaitForSeconds(portalDelay);
                }

                Instantiate(enemy, Level.I.tilemap.GetCellCenterWorld(location), Quaternion.identity,
                    Level.I.spawnedObjectsParent);
            }
        }
    }

    private Enemy SelectStartSpawnEnemy()
    {
        var totalProbability = enemies.Sum(x => x.chanceToSpawnOnCircle[Level.I.currentCircleIndex]);
        var randomValue = UnityEngine.Random.value * totalProbability;
        for (int i = 0; i < enemies.Count; i++)
        {
            var data = enemies[i];
            if (randomValue < data.chanceToSpawnOnCircle[Level.I.currentCircleIndex])
            {
                return data.enemy;
            }

            randomValue -= data.chanceToSpawnOnCircle[Level.I.currentCircleIndex];
        }

        return null;
    }

    private Enemy SelectPortalEnemy()
    {
        var availableEnemies = enemies.Where(x => x.enemy.hardness < timeHardnessMult * (Time.time - Level.I.timeOfLevelStart)).ToList();
        if (availableEnemies.Count == 0)
        {
            return null;
        }

        return availableEnemies[UnityEngine.Random.Range(0, availableEnemies.Count)].enemy;
    }

    [Serializable]
    public class EnemyData
    {
        public Enemy enemy;
        public List<float> chanceToSpawnOnCircle = new(9);
    }
}