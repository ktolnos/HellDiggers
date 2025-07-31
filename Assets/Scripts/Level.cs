using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    public static Level I;
    private Tilemap tilemap;
    
    public TileData[] usualTiles;
    public TileData goldTile;
    public TileData explosiveTile;
    public TileData wallTile;
    public int width = 100;
    public int height = 100;
    public float noiseThreshold = 0.5f;
    public float noiseFrequency = 0.01f;
    public float goldProbability = 0.01f;
    public float explosiveProbability = 0.01f;
    
    public float explosionRadius = 10f;
    public float explosionDamage = 10f;
    public float explosionDelay = 1f;
    public float explosionFPS = 10f;
    

    private Dictionary<Vector3Int, TileInfo> tileInfos = new();
    
    private void Awake()
    {
        I = this;
    }

    private void Start()
    {
        TryGetComponent(out tilemap);
        var tilePositions = new Vector3Int[width * height];
        var tiles = new TileBase[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;
                tilePositions[index] = new Vector3Int(x - width / 2, -y, 0);
                TileData tileData;
                if (x == 0 || x == width - 1)
                {
                    tileData = wallTile;
                }
                else if (Noise.GradientNoise(x * noiseFrequency, y * noiseFrequency) < noiseThreshold)
                {
                    tileData = null;
                }
                else if (UnityEngine.Random.value < explosiveProbability)
                {
                    tileData = explosiveTile;
                }
                else if (UnityEngine.Random.value < goldProbability)
                {
                    tileData = goldTile;
                }
                else
                {
                    tileData = usualTiles[UnityEngine.Random.Range(0, usualTiles.Length)];
                }
                tiles[index] = tileData?.tile;
                if (tileData != null)
                {
                    tileInfos[tilePositions[index]] = new TileInfo(tileData);
                }
            }
        }
        tilemap.SetTiles(tilePositions, tiles);
        tilemap.RefreshAllTiles();
    }

    public void Explode(Vector3 position, float radius, float damage, bool isPlayerDamage)
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
                    if (tileInfo.tileData == explosiveTile)
                    {
                        StartCoroutine(ExplodeExplosive(tilePos, tileInfo.tileData));
                    }
                    if (CheckCircleCellIntersection(position, radius, tilePos))
                    {
                        tileInfo.hp -= damage;
                        if (tileInfo.hp <= 0f)
                        {
                            RemoveTile(tilePos);
                            if (tileInfo.tileData.drop != null && UnityEngine.Random.value < tileInfo.tileData.dropChance)
                            {
                                Instantiate(tileInfo.tileData.drop, tilemap.CellToWorld(tilePos), Quaternion.identity);
                            }
                        }
                        else
                        {
                            var damagedTile = tileInfo.tileData.damagedTiles != null &&
                                           tileInfo.tileData.damagedTiles.Length > 0
                                ? tileInfo.tileData.damagedTiles[
                                    Mathf.Clamp(Mathf.FloorToInt(tileInfo.hp / tileInfo.tileData.maxHp *
                                                                 tileInfo.tileData.damagedTiles.Length),
                                        0, tileInfo.tileData.damagedTiles.Length - 1)]
                                : tileInfo.tileData.tile;
                            tilemap.SetTile(tilePos, damagedTile);
                        }
                    }
                }
            }
        }

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
                health.Damage(damage, isPlayerDamage);
            }
        }
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
        for (var i = 0; i < explosionDelay * explosionFPS; i++)
        {
            tilemap.SetTile(pos, tileData.damagedTiles[i % tileData.damagedTiles.Length]);
            yield return new WaitForSeconds(1f / explosionFPS);
        }
        RemoveTile(pos);
        Explode(tilemap.CellToWorld(pos), explosionRadius, explosionDamage, isPlayerDamage:false);
    }

    private void RemoveTile(Vector3Int pos)
    {
        tilemap.SetTile(pos, null);
        tileInfos.Remove(pos);
    }
    
    [Serializable]
    public class TileData
    {
        public TileBase tile;
        public float maxHp = 1f;
        public float dropChance = 0f;
        
        public TileBase[] damagedTiles;
        public GameObject drop;
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
