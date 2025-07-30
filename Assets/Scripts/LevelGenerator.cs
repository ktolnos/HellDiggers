using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    private Tilemap tilemap;
    
    public TileBase tile;
    public int width = 100;
    public int height = 100;

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
                tilePositions[index] = new Vector3Int(x - width / 2, y, 0);
                tiles[index] = tile;
            }
        }
        tilemap.SetTiles(tilePositions, tiles);
    }
}
