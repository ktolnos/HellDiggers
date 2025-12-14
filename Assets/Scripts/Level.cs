using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TileData = HellCircleSettings.TileData;

public class Level : MonoBehaviour
{
    public static Level I;
    public Grid grid;

    public HellCircleSettings[] circles;
    public TileData wallTile;
    public int width = 100;
    public int height = 100;
    public int wallsHeight = 100;
    public Transform spawnedObjectsParent;
    public int currentCircleIndex;
    [HideInInspector] public float timeOfRunStart;
    [HideInInspector] public float timeOfFloorStart;
    private float transitionHeight;
    public Image transitionPanel;
    public int bossHeight = 20;
    public TextMeshProUGUI circleText;
    public AudioClip blockBreak;

    private Dictionary<Vector3Int, TileInfo> tileInfos = new();
    public bool isLevelTransition;
    public SpriteRenderer levelBg;

    public int tilemapNumber;
    public int tilemapRenderDistance = 2;
    private Tilemap[] _tilemaps;
    public Tilemap tilemapPrefab;
    public Transform tilemapParent;

    public Transform pooledObjectsParent;
    public TileBase[] cracks;
    public Tilemap cracksTilemap;
    public Tilemap bgTilemap;
    public List<RoomInfo> rooms = new();
    public TileBase roomEmptyTile;
    public RoomInfo hub;

    private void Awake()
    {
        I = this;
        _tilemaps = new Tilemap[tilemapNumber];
        for (int i = 0; i < tilemapNumber; i++)
        {
            var tilemap = Instantiate(tilemapPrefab, tilemapParent);
            tilemap.gameObject.name = "Tilemap " + i;
            _tilemaps[i] = tilemap;
        }

        grid = GetComponent<Grid>();
    }

    private void Start()
    {
        PlayAgain();
    }

    public void PlayAgain()
    {
        Clear();
        timeOfRunStart = Time.time;
        currentCircleIndex = -1;
        var playerPos = Player.I.transform.position;
        Player.I.transform.position = new Vector3(0, 0, playerPos.z);
        SpawnRoom(hub, Vector2Int.zero);
        transitionPanel.gameObject.SetActive(false);
        transitionHeight = -30f;
    }

    private void GenerateLevel(HellCircleSettings circleConfig)
    {
        Clear();
        timeOfFloorStart = Time.time;
        levelBg.color = circleConfig.color;

        var bossTiles = circleConfig.boss == null ? 0 : width;
        var totalTiles = width * height + wallsHeight * 4 + bossTiles;
        var tilePositions = new Vector3Int[totalTiles];
        var tiles = new TileBase[totalTiles];
        var index = 0;
        var totalProbability = circleConfig.tileData.Sum(x => x.spawnChance);
        var totalSurfaceProbability = circleConfig.tileData.Sum(x => x.spawnChance * x.surfaceSpawnChanceMult);
        var noiseSeed = Random.Range(0, 10000);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 1 at 0, 1 at -height, 0 between -height/6 and -5*height/6 (linear)
                var thresholdSub = Mathf.Clamp01(Mathf.Abs(y - height / 2f) / height);

                tilePositions[index] = new Vector3Int(x - width / 2, -y, 0);
                var (tileData, bgTileData) = GetTile(tilePositions[index], circleConfig, noiseSeed, thresholdSub,
                    totalProbability, totalSurfaceProbability);
                tiles[index] = tileData?.tile;
                if (tileData != null)
                {
                    if (tileData.variants.Length > 0)
                    {
                        var randomIndex = Mathf.FloorToInt(Random.value * (tileData.variants.Length + 1));
                        if (randomIndex < tileData.variants.Length)
                        {
                            tiles[index] = tileData.variants[randomIndex];
                        }
                    }

                    var pos = tilePositions[index];
                    SetTile(new TileInfo(tileData, pos));
                }

                if (bgTileData != null)
                {
                    bgTilemap.SetTile(tilePositions[index], bgTileData.tile);
                }

                index++;
            }
        }

        void SetWall(Vector3Int pos)
        {
            tilePositions[index] = pos;
            tiles[index] = wallTile.tile;
            tileInfos[pos] = new TileInfo(wallTile, pos);
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
                grid.CellToWorld(new Vector3Int(0, -height - bossHeight + 3, 0)), Quaternion.identity,
                spawnedObjectsParent);
        }
        
        transitionHeight = grid.cellSize.y * (-height - wallsHeight / 2f);
        SpawnRooms();

        EnemySpawner.I.Spawner(true);
    }


    private void SpawnRooms()
    {
        var roomPositions = new List<Vector2Int>();
        for (int x = -width / 2 + 10; x < width / 2 - 1; x+=20)
        {
            for (int y = -height + 10; y < -1; y+=40)
            {
                roomPositions.Add(new Vector2Int(x, y+Random.Range(-10,10)));
            }
        }
        roomPositions = roomPositions.OrderBy(x => Random.value).ToList();
        var roomPosIndex = 0;
        foreach (var roomInfo in rooms.Where(room => room.circleIndex == currentCircleIndex))
        {
           
            for (int roomi = 0; roomi < roomInfo.amount; roomi++)
            {
                if (roomPosIndex >= roomPositions.Count)
                {
                    Debug.LogWarning("Not enough room positions for rooms");
                    break;
                }
                var startX = roomPositions[roomPosIndex].x;
                var startY = roomPositions[roomPosIndex].y;
                roomPosIndex++;
                SpawnRoom(roomInfo, new Vector2Int(startX, startY));
            }
        }
    }

    private void SpawnRoom(RoomInfo roomInfo, Vector2Int position)
    {
        roomInfo.tilemap.CompressBounds();
        var bounds = roomInfo.tilemap.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var pos = new Vector3Int(position.x + x, position.y + y, 0);
                var tile = roomInfo.tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    RemoveTile(pos);
                    if (tile != roomEmptyTile)
                    {
                        SetTile(new TileInfo(new TileData
                        {
                            tile = tile,
                            maxHp = 9999f,
                        }, pos));
                    }
                }
            }
        }

        for (int childi = 0; childi < roomInfo.tilemap.transform.childCount; childi++)
        {
            var child = roomInfo.tilemap.transform.GetChild(childi);
            Instantiate(child.gameObject, roomInfo.tilemap.LocalToWorld(child.localPosition) + grid.CellToWorld(
                    (Vector3Int)position),
                child.rotation, spawnedObjectsParent);
        }
    }

    public void Explode(Vector3 position, float radius, float enemyDamage, float groundDamage, DamageDealerType type)
    {
        var radiusCeil = Mathf.CeilToInt(radius);
        var cellPos = grid.WorldToCell(position);
        for (var x = -radiusCeil; x <= radiusCeil; x++)
        {
            for (var y = -radiusCeil; y <= radiusCeil; y++)
            {
                var tilePos = new Vector3Int(cellPos.x + x, cellPos.y + y, 0);
                var tileInfo = tileInfos.GetValueOrDefault(tilePos, null);
                if (tileInfo != null)
                {
                    if (tileInfo.tileData == wallTile)
                    {
                        continue;
                    }

                    if (CheckCircleCellIntersection(position, radius, tilePos))
                    {
                        if (radius > 1f && CheckCircleCellIntersection(position, radius - 1f, tilePos))
                        {
                            bgTilemap.SetTile(tilePos, null);
                        }

                        tileInfo.hp -= groundDamage;
                        if (tileInfo.hp <= 0f)
                        {
                            SoundManager.I.PlaySfx(blockBreak, grid.GetCellCenterWorld(tilePos));
                            if (tileInfo.tileData.drop != null && Random.value < tileInfo.tileData.dropChance)
                            {
                                var rotation = tileInfo.tileData.randomRotation ? Random.Range(0, 360f) : 0f;
                                var quaternion = Quaternion.Euler(0f, 0f, rotation);
                                var pool = GameObjectPoolManager.I.GetOrRegisterPool(tileInfo.tileData.drop,
                                    pooledObjectsParent);
                                pool.InstantiateTemporarily(grid.GetCellCenterWorld(tilePos), quaternion,
                                    tileInfo.tileData.lootLifetime);
                            }

                            RemoveTile(tilePos); // has to be after instantiate so drop can get the tileInfo
                        }
                        else
                        {
                            SetTile(tileInfo);
                        }
                    }
                }
            }
        }

        GM.DamageEntities(position, radius, enemyDamage, type);
    }

    private (TileData, TileData) GetTile(Vector3Int pos,
        HellCircleSettings circleConfig,
        int noiseSeed,
        float thresholdSub,
        float totalProbability,
        float totalSurfaceProbability
    )
    {
        var x = pos.x;
        var y = pos.y;
        if (x == -width / 2 || x == width / 2 - 1)
        {
            return (wallTile, null);
        }

        var noise = Noise.GradientNoise(x * circleConfig.noiseFrequency, y * circleConfig.noiseFrequency, noiseSeed);
        var bgTile = noise < circleConfig.noiseThreshold + thresholdSub - 0.1f ? null : circleConfig.tileData[0];
        if (noise < circleConfig.noiseThreshold + thresholdSub)
        {
            return (null, bgTile);
        }

        var isSurface = !HasTile(pos + Vector3Int.up);
        var totalProb = isSurface ? totalSurfaceProbability : totalProbability;
        var randomValue = Random.value * totalProb;
        for (int i = 0; i < circleConfig.tileData.Length; i++)
        {
            var data = circleConfig.tileData[i];
            var chance = !isSurface ? data.spawnChance : data.spawnChance * data.surfaceSpawnChanceMult;
            if (randomValue < chance)
            {
                return (data, bgTile);
            }

            randomValue -= chance;
        }

        Debug.LogWarning("No tile found for position: " + pos);
        return (null, bgTile);
    }

    public bool HasTile(Vector3Int pos)
    {
        return tileInfos.ContainsKey(pos);
    }

    public bool HasTile(Vector3 pos)
    {
        return tileInfos.ContainsKey(WorldToCell(pos));
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        worldPos.z = 0;
        return grid.WorldToCell(worldPos);
    }

    public TileInfo GetTileInfo(Vector3Int pos)
    {
        return tileInfos.GetValueOrDefault(pos, null);
    }

    public TileInfo GetTileInfo(Vector3 pos)
    {
        return GetTileInfo(grid.WorldToCell(pos));
    }

    public void RemoveBossFloor()
    {
        for (var x = -width / 2 + 1; x < width / 2 - 1; x++)
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
        var cellCenter = grid.GetCellCenterWorld(cellPos);
        var circleDistanceX = Math.Abs(cellCenter.x - position.x);
        var circleDistanceY = Math.Abs(cellCenter.y - position.y);

        if (circleDistanceX > (grid.cellSize.x / 2 + radius) ||
            circleDistanceY > (grid.cellSize.y / 2 + radius))
        {
            return false;
        }

        if (circleDistanceX <= (grid.cellSize.x / 2) ||
            circleDistanceY <= (grid.cellSize.y / 2))
        {
            return true;
        }

        var cornerDistanceSq = (circleDistanceX - grid.cellSize.x / 2) *
                               (circleDistanceX - grid.cellSize.x / 2) +
                               (circleDistanceY - grid.cellSize.y / 2) *
                               (circleDistanceY - grid.cellSize.y / 2);

        return cornerDistanceSq <= (radius * radius);
    }


    private int GetTilemapIdx(Vector3Int pos)
    {
        var index = -pos.y * tilemapNumber / height;
        index = Mathf.Clamp(index, 0, tilemapNumber - 1);
        return index;
    }


    private Tilemap GetTilemap(Vector3Int pos)
    {
        return _tilemaps[GetTilemapIdx(pos)];
    }

    public void SetTile(TileInfo tileInfo)
    {
        var pos = tileInfo.pos;
        var tile = tileInfo.tileData.tile;

        tileInfos[tileInfo.pos] = tileInfo;
        var tm = GetTilemap(pos);
        tm.SetTile(pos, tile);
        tm.SetTileFlags(pos, TileFlags.None);
        if (tileInfo.tileData.allowLava)
        {
            tm.SetColor(pos, Color.black);
        }
        else
        {
            tm.SetColor(pos, Color.white);
        }
        if (tileInfo.hp == tileInfo.tileData.maxHp)
        {
            cracksTilemap.SetTile(pos, null);
        }
        else
        {
            var crackTile = cracks[Mathf.Clamp(Mathf.FloorToInt((1f - tileInfo.hp / tileInfo.tileData.maxHp) *
                                                                cracks.Length), 0, cracks.Length - 1)];
            cracksTilemap.SetTile(pos, crackTile);
        }
    }

    private void RemoveTile(Vector3Int pos)
    {
        GetTilemap(pos).SetTile(pos, null);
        cracksTilemap.SetTile(pos, null);
        tileInfos.Remove(pos);
    }

    private void Update()
    {
        var playerPos = Player.I.transform.position;
        if (playerPos.y < transitionHeight && !isLevelTransition && Player.I.health.currentHealth > 0)
        {
            AnimateNextLevel(skipIn: false);
        }

        var playerTilemapIdx = GetTilemapIdx(grid.WorldToCell(playerPos));
        for (int i = 0; i < _tilemaps.Length; i++)
        {
            _tilemaps[i].gameObject.SetActive(Mathf.Abs(i - playerTilemapIdx) <= tilemapRenderDistance);
        }
    }

    private void AnimateNextLevel(bool skipIn)
    {
        var playerPos = Player.I.transform.position;
        isLevelTransition = true;
        currentCircleIndex++;
        currentCircleIndex = Mathf.Clamp(currentCircleIndex, 0, circles.Length - 1);
        circleText.text = circles[currentCircleIndex].circleName;
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

            transitionPanel.DOFade(0f, animationDuration).SetDelay(0.7f).OnComplete(() =>
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
        foreach (var tilemap in _tilemaps)
        {
            tilemap.ClearAllTiles();
        }

        cracksTilemap.ClearAllTiles();
        bgTilemap.ClearAllTiles();

        for (int i = spawnedObjectsParent.childCount - 1; i >= 0; i--)
        {
            var child = spawnedObjectsParent.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = pooledObjectsParent.childCount - 1; i >= 0; i--)
        {
            var child = pooledObjectsParent.GetChild(i);
            if (child != null && child.gameObject.activeInHierarchy)
            {
                GameObjectPoolManager.I.Release(child.gameObject);
            }
        }
    }

    public class TileInfo
    {
        public TileData tileData;
        public float hp;
        public Vector3Int pos;

        public TileInfo(TileData tileData, Vector3Int position)
        {
            this.tileData = tileData;
            hp = tileData.maxHp;
            pos = position;
        }
    }

    [Serializable]
    public class RoomInfo
    {
        public Tilemap tilemap;
        public int circleIndex;
        public int amount;
    }
}

public enum DamageDealerType
{
    Player,
    Enemy,
    Environment
}