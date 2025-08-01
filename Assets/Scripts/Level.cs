using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    public static Level I;
    public Tilemap tilemap;
    
    public CircleConfig[] circles;
    public TileData wallTile;
    public int width = 100;
    public int height = 100;
    public int wallsHeight = 100;
    public float damagedProb = 0.1f;
    public int currentCircleIndex = 0;
    private float transitionHeight;
    

    private Dictionary<Vector3Int, TileInfo> tileInfos = new();
    
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    private void Awake()
    {
        I = this;
        TryGetComponent(out tilemap);
    }

    private void Start()
    {
        PlayAgain();
    }

    public void PlayAgain()
    {
        currentCircleIndex = 0;
        GenerateLevel(circles[currentCircleIndex]);
        foreach (var spawnedObject in spawnedObjects)
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }
        }
    }

    private void GenerateLevel(CircleConfig circleConfig)
    {
        tileInfos.Clear();
        tilemap.ClearAllTiles();
        
        var tilePositions = new Vector3Int[width * height + wallsHeight * 4];
        var tiles = new TileBase[width * height + wallsHeight * 4];
        var index = 0;
        var totalProbability = circleConfig.tileData.Sum(x => x.spawnChance);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tilePositions[index] = new Vector3Int(x - width / 2, -y, 0);
                TileData tileData = null;
                if (x == 0 || x == width - 1)
                {
                    tileData = wallTile;
                }
                else if (Noise.GradientNoise(x * circleConfig.noiseFrequency, y * circleConfig.noiseFrequency) <
                         circleConfig.noiseThreshold)
                {
                    tileData = null;
                }
                else
                {
                    var randomValue = UnityEngine.Random.value * totalProbability;
                    for (int i = 0; i < circleConfig.tileData.Length; i++)
                    {
                        var data = circleConfig.tileData[i];
                        if (randomValue < data.spawnChance)
                        {
                            tileData = data;
                            break;
                        }
                        randomValue -= data.spawnChance;
                    }
                }
                tiles[index] = tileData?.tile;
                if (tileData != null)
                {
                    tileInfos[tilePositions[index]] = new TileInfo(tileData);
                    if (tileData.maxHp > 1 && UnityEngine.Random.value < damagedProb)
                    {
                        tileInfos[tilePositions[index]].hp = tileData.maxHp * UnityEngine.Random.Range(0.1f, 0.9f);
                        tiles[index] = GetDamagedTile(tileInfos[tilePositions[index]]);
                    }
                }
                index++;
            }
        }

        void SetWall(Vector3Int pos)
        {
            tilePositions[index] = pos;
            tiles[index] = wallTile.tile;
            tileInfos[tilePositions[index]] = new TileInfo(wallTile);
            index++;
        }
        
        for (int i = 0; i < wallsHeight; i++)
        {
            SetWall(new Vector3Int(-width / 2, i, 0));
            SetWall(new Vector3Int(width / 2 - 1, i, 0));
            SetWall(new Vector3Int(-width / 2, -height - i, 0));
            SetWall(new Vector3Int(width / 2 - 1, -height - i, 0));
        }
        tilemap.SetTiles(tilePositions, tiles);
        tilemap.RefreshAllTiles();
        transitionHeight = tilemap.layoutGrid.cellSize.y * (-height - wallsHeight / 2f);
    }
    public static void DamageEntities(Vector3 position, float radius, float damage, DamageDealerType type)
    {
        var colliders = Physics2D.OverlapCircleAll(position, radius);
        var healths = new HashSet<Health>();
        foreach (var collider in colliders)
        {
            healths.Add(collider.GetComponentInParent<Health>());
        }
        foreach (var health in healths)
        {
            if (health != null)
            {
                health.Damage(damage, type);
            }
        }
    }

    public void Explode(Vector3 position, float radius, float enemyDamage, float groundDamage, DamageDealerType type)
    {
        var radiusCeil = Mathf.CeilToInt(radius);
        var cellPos = tilemap.layoutGrid.WorldToCell(position);
        for (var x = -radiusCeil; x <= radiusCeil; x++)
        {
            for (var y = -radiusCeil; y <= radiusCeil; y++) {
                var tilePos = new Vector3Int(cellPos.x + x, cellPos.y + y, 0);
                var tileInfo = tileInfos.GetValueOrDefault(tilePos, null);
                if (tileInfo != null)
                {
                    if (tileInfo.tileData == wallTile)
                    {
                        continue;
                    }
                    if (tileInfo.tileData.explosionRadius > 0f)
                    {
                        if (tileInfo.hp > 0f && tileInfo.hp <= groundDamage)
                        {
                            tileInfo.hp = 0f;
                            StartCoroutine(ExplodeExplosive(tilePos, tileInfo.tileData));
                        }
                        else
                        {
                            tileInfo.hp -= groundDamage;
                        }
                        continue;
                    }
                    if (CheckCircleCellIntersection(position, radius, tilePos))
                    {
                        tileInfo.hp -= groundDamage;
                        if (tileInfo.hp <= 0f)
                        {
                            RemoveTile(tilePos);
                            if (tileInfo.tileData.drop != null && UnityEngine.Random.value < tileInfo.tileData.dropChance)
                            {
                                var spawned = Instantiate(tileInfo.tileData.drop, tilemap.CellToWorld(tilePos), Quaternion.identity);
                                spawnedObjects.Add(spawned);
                            }
                        }
                        else
                        {
                            var damagedTile = GetDamagedTile(tileInfo);
                            tilemap.SetTile(tilePos, damagedTile);
                        }
                    }
                }
            }
        }

        DamageEntities(position, radius, enemyDamage, type);
    }

    private TileBase GetDamagedTile(TileInfo tileInfo)
    {
        return tileInfo.tileData.damagedTiles != null &&
                          tileInfo.tileData.damagedTiles.Length > 0
            ? tileInfo.tileData.damagedTiles[
                Mathf.Clamp(Mathf.FloorToInt((1f - tileInfo.hp / tileInfo.tileData.maxHp) *
                                             tileInfo.tileData.damagedTiles.Length),
                    0, tileInfo.tileData.damagedTiles.Length - 1)]
            : tileInfo.tileData.tile;
    }
    
    private bool CheckCircleCellIntersection(Vector3 position, float radius, Vector3Int cellPos)
    {
        var cellCenter = tilemap.layoutGrid.GetCellCenterWorld(cellPos);
        var circleDistanceX = Math.Abs(cellCenter.x - position.x);
        var circleDistanceY = Math.Abs(cellCenter.y - position.y);
        
        if (circleDistanceX > (tilemap.layoutGrid.cellSize.x / 2 + radius) ||
            circleDistanceY > (tilemap.layoutGrid.cellSize.y / 2 + radius))
        {
            return false;
        }
        
        if (circleDistanceX <= (tilemap.layoutGrid.cellSize.x / 2) ||
            circleDistanceY <= (tilemap.layoutGrid.cellSize.y / 2))
        {
            return true;
        }
        
        var cornerDistanceSq = (circleDistanceX - tilemap.layoutGrid.cellSize.x / 2) *
                               (circleDistanceX - tilemap.layoutGrid.cellSize.x / 2) +
                               (circleDistanceY - tilemap.layoutGrid.cellSize.y / 2) *
                               (circleDistanceY - tilemap.layoutGrid.cellSize.y / 2);
        
        return cornerDistanceSq <= (radius * radius);
    }

    private IEnumerator ExplodeExplosive(Vector3Int pos, TileData tileData)
    {
        for (var i = 0; i < tileData.explosionDelay * tileData.explosionFPS; i++)
        {
            tilemap.SetTile(pos, tileData.damagedTiles[i % tileData.damagedTiles.Length]);
            yield return new WaitForSeconds(1f / tileData.explosionFPS);
        }
        RemoveTile(pos);
        Explode(tilemap.CellToWorld(pos), tileData.explosionRadius, tileData.explosionDamage,  tileData.explosionDamage,  DamageDealerType.Environment);
    }

    private void RemoveTile(Vector3Int pos)
    {
        tilemap.SetTile(pos, null);
        tileInfos.Remove(pos);
    }

    private void Update()
    {
        var playerPos = Player.I.transform.position;
        if (playerPos.y < transitionHeight)
        {
            Player.I.transform.position = new Vector3(playerPos.x, wallsHeight / 2f, playerPos.z);
            currentCircleIndex++;
            currentCircleIndex = Mathf.Clamp(currentCircleIndex, 0, circles.Length - 1);
            GenerateLevel(circles[currentCircleIndex]);
        }
    }
    
    [Serializable]
    public class CircleConfig
    {
        public TileData[] tileData;
        public float noiseThreshold = -0.2f;
        public float noiseFrequency = 0.15f;
    }

    [Serializable]
    public class TileData
    {
        public string name;
        public TileBase tile;
        public float maxHp = 1f;
        public float spawnChance = 1f;
        
        public float explosionRadius = 0f;
        public float explosionDamage = 0f;
        public float explosionDelay = 1f;
        public float explosionFPS = 10f;
        
        public TileBase[] damagedTiles;
        public GameObject drop;
        public float dropChance = 0f;
    }
    
    private class TileInfo
    {
        public TileData tileData;
        public float hp;
        
        public TileInfo(TileData tileData)
        {
            this.tileData = tileData;
            hp = tileData.maxHp;
        }
    }
}

public enum DamageDealerType
{
    Player,
    Enemy,
    Environment
}