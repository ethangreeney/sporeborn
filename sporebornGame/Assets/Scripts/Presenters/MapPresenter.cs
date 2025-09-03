using System.Collections.Generic;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private float cellSize = 1f;

    private MapModel model;
    private RoomDataLoader RoomLoader;
    private RoomData Rooms;


    public GameObject WallPrefab;
    public GameObject DoorPrefab;
    public GameObject ItemPrefab;
    public GameObject SpawnableTile;

    private List<Cell> SpawnedCells;
    public TextAsset tileMapJsonFile;

    void Start()
    {
        // Generates new map
        model = new MapModel(10, 20);

        SpawnedCells = model.GetCellList;

        // Load room data from json file

    }

    // Called when entering a new room
    public void BuildRoom(RoomShape shape, RoomType type)
    {
        // Get the data of the room from the json file
        RoomData TileData = RoomLoader.GetRoomData(shape, type);

        // Spawn room from json file using tile set
        foreach (TileData tile in Rooms.RoomTiles)
        {

            Vector3 location = IndexToCoordinate(tile.Index);

            if (tile.IsWall)
            {
                Instantiate(WallPrefab, location, Quaternion.identity);
            }
            else if (tile.IsDoor)
            {
                Instantiate(DoorPrefab, location, Quaternion.identity);
            }
            else if (tile.IsItem)
            {
                Instantiate(ItemPrefab, location, Quaternion.identity);
            }
            else
            {
                // Is an enemy/player spawnable/pathfinding tile
                Instantiate(SpawnableTile, location, Quaternion.identity);
            }
        }
        // TODO: Add door placement
        // Add doors based on a whether there is a neighbouring room
        // Each room will have a North,East,South,West possible door position
        // Based on this the door will be placed 

    }
    
    // Maybe move to seperate door class
    public void OpenDoor()
    {
        // Player collides with door 
        // Delete exisiting room 
        // Create new room 

        // Keep player position but offset it by room size
    }

    public void SpawnEnemies()
    {
        
    }

    public void SpawnBoss()
    {
        
    }

    public Vector3 IndexToCoordinate(int index)
    {
        int x = index % 10;
        int y = index / 10;
        Vector3 cooordinate = new(x, y, 0);

        return cooordinate;
    }
}