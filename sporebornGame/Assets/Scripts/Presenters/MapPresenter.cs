using System.Collections.Generic;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private float cellSize = 1f;

    private MapModel model;
    private List<Cell> SpawnedCells;

    public TextAsset tileMapJsonFile;

    void Start()
    {
        // Generates new map
        model = new MapModel(10, 20);

        SpawnedCells = model.GetCellList;

        // Load room data from json file

    }

    public void BuildRoom(int index, RoomShape shape, RoomType type)
    {
        // Get the data of the room from the json file
        RoomData data = RoomDataLoader.GetRoomData(index, shape, type);


        // Spawn room from json file using tile set

        // Add doors based on a whether there is a neighbouring room
        // Each room will have a North,East,South,West possible door position
        // Based on this the door will be placed 

    }

    public void OpenDoor()
    {
        // Player collides with door 
        // Delete exisiting room 
        // Create new room 

        // Keep player position but offset it by room size
    }
}