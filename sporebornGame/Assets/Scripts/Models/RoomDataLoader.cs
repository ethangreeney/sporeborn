using System.Collections.Generic;
using UnityEngine; // Used for JsonUtility
using System.IO;

public class RoomDataLoader
{
    // Deserializing from a JSON File
    public RoomData GetRoomData(int index, RoomShape shape, RoomType type)
    {
        string FilePath = GetFilePath(shape, type);
        string jsonString = File.ReadAllText(FilePath);

        RoomData data = JsonUtility.FromJson<RoomData>(jsonString)
            ?? throw new System.Exception("Failed to deserialise RoomData from JSON file at: " + FilePath);

        return data;
    }

    // Using Consistent naming conventions, E.g. Rooms/LShape_0_Regular.json
    public string GetFilePath(RoomShape shape, RoomType type)
    {
        return "Rooms/" + shape.ToString() + "_" + type.ToString() + ".json";
    }
}