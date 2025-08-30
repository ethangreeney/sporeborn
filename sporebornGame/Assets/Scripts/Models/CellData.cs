using System.Collections.Generic;
using UnityEngine;


public enum RoomType
{
    Regular,
    Item,
    Shop,
    Boss,
    Secret
}

public enum RoomShape
{
    OneByOne,
    OneByTwo,
    TwoByOne,
    TwoByTwo,
    LShape
}

public class CellData : Monobehaviour
{

    private RoomType roomType;
    public RoomType RoomTypeReference => roomType;

    private RoomShape roomShape;
    public RoomType RoomShapeReference => roomShape;

    public int index;
    public int value;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer roomSprite;

    // Indexes that are attached to the Cell Object
    public List<int> cellList = new List<int>();

    public void SetRoomType(RoomType newRoom)
    {
        roomType = newRoom;
    }

    public void setRoomShape(RoomShape newRoom)
    {
        roomShape = newRoom;
    }

    public void setSpecialRoomSprite(Sprite icon)
    {
        spriteRenderer.sprite = icon;
    }

    public void setLargeRoomSprite(Sprite roomIcon)
    {
        roomSprite.sprite = roomIcon;
    }

    public void rotateCell(List<int> connectedCells)
    {
        // Ensure the cells are in order
        connectedCells.Sort();
        index = connectedCells[0];

        // Changes the orientation of the sprite 
        // Compares the value (0 or 1) with the surrouding Cells to see if it needs to be rotated

        // (index - 11)   (index - 10)   (index - 9)
        // (index - 1)    (index)        (index + 1)
        // (index + 9)    (index + 10)   (index + 11)

        // Checks if the right cell and the cell below exist, as a cell already on map
        if (connectedCells.Contains(index + 1) && connectedCells.Contains(index + 10))
        {
            ApplyRotation(-90);
        }
        // Checks if the right cell and the cell below exist and to the right, as a cell already on map
        else if (connectedCells.Contains(index + 1) && connectedCells.Contains(index + 11))
        {
            ApplyRotation(180);
        }
        // Checks if the cell below and to the left and cell below exist, as a cell already on map
        else if (connectedCells.Contains(index + 9) && connectedCells.Contains(index + 10))
        {
            ApplyRotation(180);
        }
    }

    public void ApplyRotation(float angle)
    {
        // Sets the z rotation to input angle
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
