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
        // Generates new map
        model = new MapModel(10, 20);

        SpawnedCells = model.GetCellList;

        // Load room data from json file

    }

    public void BuildLevel()
    {
        Vector2 RoomCoordinates;

        foreach (Cell cell in SpawnedCells) {
            RoomCoordinates = IndexToCoodinate(cell.Index);
            Instantiate(cell, IndexToPosition, Quaternion.identity);
        }
    }

    private Vector2 IndexToCoodinate(int index)
    {
        int x = index % 10;
        int y = index / 10;
        return new Vector2(x * cellSize, -y * cellSize);
    }
}