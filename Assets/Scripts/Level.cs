using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    public static Level I;
    private Tilemap tilemap;
    
    public TileBase tile;
    public int width = 100;
    public int height = 100;

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
                tiles[index] = tile;
            }
        }
        tilemap.SetTiles(tilePositions, tiles);
        tilemap.RefreshAllTiles();
    }

    public void Explode(Vector3 position, float radius)
    {
        var radiusCeil = Mathf.CeilToInt(radius);
        var cellPos = tilemap.layoutGrid.WorldToCell(position);
        for (var x = -radiusCeil; x <= radiusCeil; x++)
        {
            for (var y = -radiusCeil; y <= radiusCeil; y++) {
                var tilePos = new Vector3Int(cellPos.x + x, cellPos.y + y, 0);
                if (tilemap.HasTile(tilePos))
                {
                    if (CheckCircleCellIntersection(position, radius, tilePos))
                    {
                        tilemap.SetTile(tilePos, null);
                    }
                }
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
}
