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
    private float _timeOfLastSpawn;
    public List<int> numPrespawnedEnemies = new();
    public List<float> spawnDelays = new();
    public List<float> hardnessMultipliers = new();
    public float spawnsPerHardness = 0.01f;
    private float currentHardness;
    private bool spawnedNoAmmoDeathEnemy = false;

    private HashSet<Enemy> spawnedEnemies = new();

    private void Awake()
    {
        I = this;
    }

    void Start()
    {
        _timeOfLastSpawn = Time.time;
    }

    void Update()
    {
        if (Player.I.health.currentHealth <= 0 || Level.I.currentCircleIndex < 0)
        {
            return;
        }

        if (Level.I.isLevelTransition)
        {
            _timeOfLastSpawn = Time.time;
            currentHardness = 0;
        }

        var enemyStartTime = Time.time - Level.I.timeOfFloorStart - spawnDelays[Level.I.currentCircleIndex];
       
        currentHardness = hardometer * enemyStartTime * hardnessMultipliers[Level.I.currentCircleIndex];

        if (Player.I.gun.AmmoInMagLeft <= 0 && Player.I.gun.AmmoOutOfMagLeft <= 0 && !spawnedNoAmmoDeathEnemy)
        {
            spawnedNoAmmoDeathEnemy = true;
            SpawnEnemy(true, enemies[0].enemy);
        }
        
        if (enemyStartTime < 0)
        {
            return;
        }
        

        if (Time.time - _timeOfLastSpawn > 1f / spawnsPerHardness / currentHardness)
        {
            Spawner(false);
            _timeOfLastSpawn = Time.time;
        }

        if (Level.I.isInBossRoom)
        {
            foreach (var spawnedEnemy in spawnedEnemies)
            {
                if (spawnedEnemy != null)
                {
                    Destroy(spawnedEnemy.gameObject);
                }
            }
            spawnedEnemies.Clear();
        }
    }

    public void Spawner(bool startSpawn)
    {
        var amount = enemyAmount;
        if (startSpawn)
        {
            amount = numPrespawnedEnemies[Level.I.currentCircleIndex];
            Reset();
        }

        for (int i = 0; i < amount; i++)
        {
            StartCoroutine(SpawnEnemy(fromPortal:!startSpawn,
                enemy:startSpawn? SelectStartSpawnEnemy(): SelectPortalEnemy()));
        }

        if (startSpawn)
        {
            StartCoroutine(SpawnDelayedEnemies());
        }
    }

    public IEnumerator SpawnEnemy(bool fromPortal, Enemy enemy)
    {
        if (Level.I.isInBossRoom)
        {
            yield break;
        }
        var foundLocation = false;
        var location = Vector3Int.zero;
        var count = 0;

        while (count < 100)
        {
            if (!fromPortal)
            {
                var left = -Level.I.width / 2f + 5f;
                var right = Level.I.width / 2f - 5f;
                var height = -Level.I.height;
                location = new Vector3Int(Mathf.RoundToInt(UnityEngine.Random.Range(left, right)),
                    Mathf.RoundToInt(UnityEngine.Random.Range(-40, height + 20)), 0);
            }
            else
            {
                var playerPos = Level.I.grid.WorldToCell(Player.I.gameObject.transform.position);
                location = new Vector3Int(
                    UnityEngine.Random.Range(
                        -spawnRadius + playerPos.x,
                        spawnRadius + playerPos.x),
                    UnityEngine.Random.Range(
                        -spawnRadius + playerPos.y,
                        spawnRadius * 2 + playerPos.y), 0);
            }

            if (!Level.I.HasTile(location))
            {
                foundLocation = true;
                break;
            }

            count++;
            if (count >= 100)
            {
                Debug.Log("Could not find location to spawn enemy");
            }
        }

        if (foundLocation)
        {
            if (enemy != null)
            {
                if (fromPortal)
                {
                    Destroy(
                        Instantiate(portal, Level.I.grid.GetCellCenterWorld(location), Quaternion.identity,
                            Level.I.spawnedObjectsParent), portalDelay);
                    yield return new WaitForSeconds(portalDelay);
                }

                if (Level.I.isInBossRoom)
                {
                    yield break;
                }
                var enemyPos = Level.I.grid.GetCellCenterWorld(location);
                enemyPos.z = enemy.transform.position.z;
                var enemyInstance = Instantiate(enemy, enemyPos, Quaternion.identity,
                    Level.I.spawnedObjectsParent);
                enemyInstance.isAgro |= fromPortal;
                spawnedEnemies.Add(enemyInstance);
            }
        }
    }

    private Enemy SelectStartSpawnEnemy()
    {
        var startSpawnEnemies = enemies.Where(x => x.prespawn && 
                                                   x.chanceToSpawnOnCircle[Level.I.currentCircleIndex] > 0).ToList();
        var totalProbability = startSpawnEnemies.Sum(x => x.chanceToSpawnOnCircle[Level.I.currentCircleIndex]);
        var randomValue = UnityEngine.Random.value * totalProbability;
        for (int i = 0; i < startSpawnEnemies.Count; i++)
        {
            var data = startSpawnEnemies[i];
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
        var availableEnemies = enemies.Where(x => x.enemy.hardness < 
            currentHardness && x.spawnFromPortals).ToList();
        if (availableEnemies.Count == 0)
        {
            return null;
        }

        var enemy = availableEnemies[UnityEngine.Random.Range(0, availableEnemies.Count)].enemy;
        return enemy;
    }

    private IEnumerator SpawnDelayedEnemies()
    {
        foreach (var enemyData in enemies)
        {
            if (enemyData.spawnFixedDelayed)
            {
                yield return new WaitForSeconds(enemyData.spawnFixedDelay);
                yield return StartCoroutine(SpawnEnemy(fromPortal:true, enemyData.enemy));
            }
        }
    }

    public void Reset()
    {
        StopAllCoroutines();
        spawnedNoAmmoDeathEnemy = false;
    }

    [Serializable]
    public class EnemyData
    {
        public Enemy enemy;
        public List<float> chanceToSpawnOnCircle = new(9);
        public bool prespawn;
        public bool spawnFromPortals;
        public float spawnFixedDelay;
        public bool spawnFixedDelayed;
    }
}