using System.Collections.Generic;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private float cellSize = 1f;

    private MapModel model;



    public GameObject WallPrefab;
    public GameObject DoorPrefab;
    public GameObject ItemPrefab;
    public GameObject SpawnableTile;

    private List<Cell> SpawnedCells;
    public TextAsset tileMapJsonFile;

    [Header("Room Prefabs")]
    public List<GameObject> RoomPrefabs;

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
        foreach (GameObject prefab in RoomPrefabs) {
            if (shape.Equals(prefab.ToString())
            {
                
            }
        }
        // Place room first
        if (RoomPrefabs.Count > 0)
        {
            Instantiate(RoomPrefabs[0], Vector3.zero, Quaternion.identity);
        }


        // The place door
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