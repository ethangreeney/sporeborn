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
    }

    public void BuildDungeon()
    {
        model.GenerateDungon();
        SpawnedCells = model.GetCellList;
        PlaceRooms();
    }

    public void PlaceRooms()
    {
        foreach (Cell cell in SpawnedCells) {
            Instantiate(cell, );
        }
        
    }

    private Vector2 IndexToPosition(int index)
    {
        int x = index % 10;
        int y = index / 10;
        return new Vector2(x * cellSize, -y * cellSize);
    }
}