using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class if for storing all of our information about a singular tile in the map
/// </summary>
public class Tile
{
    public int Value; //This can be used to denote if a tile is empty, an obstacle, or otherwise
    //0: Empty
    //1: Obstacle
    //more to come?

    public bool Empty => Value == 0; //A simple bool property for shorthand checking empty tiles

    //Constructor, all tiles should have a value set intentionally
    public Tile(int value)
    {
        Value = value;
    }
}
