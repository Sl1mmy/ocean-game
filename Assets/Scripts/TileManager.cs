using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject tilePrefab;  // Prefab for the ocean tile
    public int tileSize = 10;      // Size of each tile
    public int radius = 2;         // Number of tiles around the player

    [Header("Player")]
    public Transform player;       // Reference to the player

    private Vector2Int playerGridPos;
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        playerGridPos = GetPlayerGridPosition();
        GenerateInitialTiles();
    }

    void Update()
    {
        // Check if the player has moved to a new grid cell
        Vector2Int newGridPos = GetPlayerGridPosition();
        if (newGridPos != playerGridPos)
        {
            playerGridPos = newGridPos;
            UpdateTiles();
        }
    }

    void GenerateInitialTiles()
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                Vector2Int tileCoords = new Vector2Int(x, z);
                Vector3 worldPosition = GetWorldPosition(tileCoords);
                GameObject tile = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);

                tiles.Add(tileCoords, tile);
            }
        }
    }

    void UpdateTiles()
    {
        HashSet<Vector2Int> newGridPositions = new HashSet<Vector2Int>();

        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                Vector2Int gridPosition = new Vector2Int(playerGridPos.x + x, playerGridPos.y + z);
                newGridPositions.Add(gridPosition);
            }
        }

        List<Vector2Int> positionsToRecycle = new List<Vector2Int>();

        foreach (var tile in tiles)
        {
            if (!newGridPositions.Contains(tile.Key))
            {
                positionsToRecycle.Add(tile.Key);
            }
            else
            {
                newGridPositions.Remove(tile.Key);
            }
        }

        foreach (var oldPosition in positionsToRecycle)
        {
            if (newGridPositions.Count == 0) break;

            Vector2Int newPosition = GetNextGridPosition(newGridPositions);
            GameObject tile = tiles[oldPosition];

            tile.transform.position = GetWorldPosition(newPosition);

            tiles.Remove(oldPosition);
            tiles.Add(newPosition, tile);

        }
    }


    Vector3 GetWorldPosition(Vector2Int gridCoords)
    {
        return new Vector3(gridCoords.x * tileSize, 0, gridCoords.y * tileSize);
    }

    Vector2Int GetPlayerGridPosition()
    {
        int gridX = Mathf.FloorToInt(player.position.x / tileSize);
        int gridY = Mathf.FloorToInt(player.position.z / tileSize);
        return new Vector2Int(gridX, gridY);
    }

    Vector2Int GetNextGridPosition(HashSet<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            positions.Remove(pos);
            return pos;
        }
        throw new System.Exception("No available positions for tile recycling.");
    }
}
