using System.Collections.Generic;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private float cellSize = 1f;

    private MapModel model;
    private List<Cell> SpawnedCells;

    void Start()
    {
        model = new MapModel(10, 20);
        // Load sprite from json file
    }

    public void BuildLevel()
    {
        Vector2 RoomCoordinates;

        foreach (Cell cell in SpawnedCells) {
            RoomCoordinates = IndexToPosition(cell.Index);
            Instantiate(cell, IndexToPosition, Quaternion.identity);
        }
    }

    private Vector2 IndexToPosition(int index)
    {
        int x = index % 10;
        int y = index / 10;
        return new Vector2(x * cellSize, -y * cellSize);
    }
}