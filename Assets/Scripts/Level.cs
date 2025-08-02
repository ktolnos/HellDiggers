using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TileData = HellCircleSettings.TileData;

public class Level : MonoBehaviour
{
    public static Level I;
    public Tilemap tilemap;
    
    public HellCircleSettings[] circles;
    public TileData wallTile;
    public int width = 100;
    public int height = 100;
    public int wallsHeight = 100;
    public float damagedProb = 0.1f;
    public Transform spawnedObjectsParent;
    public int currentCircleIndex = 0;
    public float timeOfLevelStart;
    private float transitionHeight;
    public int startEnemyAmount;
    public Image transitionPanel;
    public int bossHeight = 20;
    public TextMeshProUGUI circleText;
    public BombAnimator bompExplosionPrefab;


    private Dictionary<Vector3Int, TileInfo> tileInfos = new();
    public bool isLevelTransition;

    private List<IEnumerator> explosionsCoroutines = new();
    
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
        timeOfLevelStart = Time.time;
        currentCircleIndex = 0;
        var playerPos = Player.I.transform.position;
        Player.I.transform.position = new Vector3(playerPos.x, wallsHeight / 2f, playerPos.z);
        GenerateLevel(circles[currentCircleIndex]);
        currentCircleIndex--;
        AnimateNextLevel(skipIn:true);
    }

    private void GenerateLevel(HellCircleSettings circleConfig)
    {
        Clear();
        EnemySpawner.I.Spawner(0f, 0, 0f, 40, true);

        var bossTiles = circleConfig.boss == null ? 0 : width;
        var totalTiles = width * height + wallsHeight * 4 + bossTiles;
        var tilePositions = new Vector3Int[totalTiles];
        var tiles = new TileBase[totalTiles];
        var index = 0;
        var totalProbability = circleConfig.tileData.Sum(x => x.spawnChance);
        var noiseSeed = UnityEngine.Random.Range(0, 10000);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var thresholdSub = y < height/2  ? 1f - Mathf.Clamp01((float) y / height) : Mathf.Clamp01((float)y / height);
                thresholdSub -= 0.5f;
                tilePositions[index] = new Vector3Int(x - width / 2, -y, 0);
                TileData tileData = null;
                if (x == 0 || x == width - 1)
                {
                    tileData = wallTile;
                }
                else if (Noise.GradientNoise(x * circleConfig.noiseFrequency, y * circleConfig.noiseFrequency, noiseSeed) <
                         circleConfig.noiseThreshold + thresholdSub)
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
                    if (tileData.variants.Length > 0)
                    {
                        var randomIndex = Mathf.FloorToInt(UnityEngine.Random.value * (tileData.variants.Length + 1));
                        if (randomIndex < tileData.variants.Length)
                        {
                            tiles[index] = tileData.variants[randomIndex];
                        }
                    }
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

        if (bossTiles != 0)
        {
            for (int x = 0; x < bossTiles; x++)
            {
                SetWall(new Vector3Int(x - width / 2, -height - bossHeight, 0));
            }
            Instantiate(circleConfig.boss, 
                tilemap.CellToWorld(new Vector3Int(0, -height - bossHeight + 3, 0)), Quaternion.identity, spawnedObjectsParent);
            Debug.Log("Instantiated boss");
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
                            var enumerator = ExplodeExplosive(tilePos, tileInfo.tileData);
                            explosionsCoroutines.Add(enumerator);
                            StartCoroutine(enumerator);
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
                                Instantiate(tileInfo.tileData.drop, tilemap.CellToWorld(tilePos), Quaternion.identity, spawnedObjectsParent);
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

    public void RemoveBossFloor()
    {
        for (var x = -width / 2; x < width / 2; x++)
        {
            var pos = new Vector3Int(x, -height - bossHeight, 0);
            if (tileInfos.ContainsKey(pos))
            {
                RemoveTile(pos);
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
        var bombExplosion = Instantiate(bompExplosionPrefab, tilemap.GetCellCenterWorld(pos), Quaternion.identity, spawnedObjectsParent);
        bombExplosion.Explode(tileData.explosionDelay);
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
        if (playerPos.y < transitionHeight && !isLevelTransition && Player.I.health.currentHealth > 0)
        {
            AnimateNextLevel(skipIn:false);
        }
    }

    private void AnimateNextLevel(bool skipIn)
    {
        var playerPos = Player.I.transform.position;
        isLevelTransition = true;
        currentCircleIndex++;
        currentCircleIndex = Mathf.Clamp(currentCircleIndex, 0, circles.Length - 1);
        circleText.text = circles[currentCircleIndex].name;
        transitionPanel.gameObject.SetActive(true);
        var animationDuration = 0.5f;
        circleText.DOFade(1f, animationDuration);
        var inDuration = skipIn ? 0f : animationDuration;
        transitionPanel.DOFade(1f, inDuration).OnComplete(() =>
        {
            Player.I.rb.bodyType = RigidbodyType2D.Kinematic;
            Player.I.transform.position = new Vector3(playerPos.x, 40f, playerPos.z);
            GenerateLevel(circles[currentCircleIndex]);
            Player.I.rb.linearVelocityY = -20f;
            transitionPanel.DOFade(0f, animationDuration).SetDelay(1f).OnComplete(() =>
            {
                transitionPanel.gameObject.SetActive(false);
                Player.I.rb.bodyType = RigidbodyType2D.Dynamic;
                isLevelTransition = false;
            });
            circleText.DOFade(0f, animationDuration);
        });
    }

    public void Clear()
    {
        tileInfos.Clear();
        tilemap.ClearAllTiles();
        
        for (int i = spawnedObjectsParent.childCount - 1; i >= 0; i--)
        {
            var child = spawnedObjectsParent.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
        foreach (var explosionsCoroutine in explosionsCoroutines)
        {
            StopCoroutine(explosionsCoroutine);
        }
        explosionsCoroutines.Clear();
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