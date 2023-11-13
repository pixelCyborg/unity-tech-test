using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for generating and storing tile data. From here we can instantiate a 3D representation of our data
/// and get data to feed to our NavGrid class
/// </summary>
public class Map : MonoBehaviour
{
    //Our 2D array of tiles
    public Tile[,] Grid;

    [Header("Map Gen Config")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [Range(0, 100)] [SerializeField] private int _obstacleChance; //% Chance for a tile to be an obstacle

    [Header("3D Config")]
    [SerializeField] private float _scale = 1f;
    [SerializeField] private Transform _tileRoot; //Parent transform to spawn tiles under

    [Header("Prefabs")]
    [SerializeField] private GameObject _emptyTile;
    [SerializeField] private GameObject _obstacleTile;

    private void Start()
    {
        //Generate a new map on start with our config width & height
        GenerateMapWithSettings();
    }

    [NaughtyAttributes.Button]
    public void GenerateMapWithSettings() //Seperated into a parameterless method so we can call it via Button
    {
        GenerateMap(_width, _height);
    }

    public void GenerateMap(int width, int height)
    {
        //Make sure we have a valid config
        if (width == 0 || height == 0) 
            Debug.LogError("Invalid map config. Width & Height need to be > 0");

        //Create a 2D array with the provided dimensions
        Grid = new Tile[width, height];
        
        //Iterate through the array to generate tiles
        for(int x = 0; x < Grid.GetLength(0); x++) //Rows
        {
            for (int y = 0; y < Grid.GetLength(1); y++) //Columns
            {
                bool obstacle = Random.Range(0, 100) < _obstacleChance; //Should this tile be empty?
                Tile tile = new Tile(obstacle ? 1 : 0); //Create a new tile
                Grid[x, y] = tile; //Place it in the grid
            }
        }

        //After map is generated, show it in 3D
        RenderMap();
    }
    
    //Generate a 3D representation of our map
    public void RenderMap()
    {
        for(int x = 0; x < Grid.GetLength(0); x++) //Rows
        {
            for (int y = 0; y < Grid.GetLength(1); y++) //Columns
            {
                Vector3 localPos = Vector3.zero;
                localPos.x = x * _scale;
                localPos.z = y * _scale;
            }
        }
    }
}
