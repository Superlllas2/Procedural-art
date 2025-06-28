using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public RectInt Bounds;
    public List<Room> ConnectedRooms = new();
}