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
    [SerializeField] public float _scale = 1f;
    [SerializeField] private Transform _tileRoot; //Parent transform to spawn tiles under

    [Header("Prefabs")]
    [SerializeField] private GameObject[] _tilePrefabs; //Our tile prefab refs. Index corresponds to tile value

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
        if (width < 1 || height < 1)
        {
            Debug.LogError("Invalid map config. Width & Height need to be > 0");
            return;
        }

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
        if(_tileRoot == null)
        {
            Debug.LogError("No tile root transform found, please assign one");
            return;
        }
        if(Grid == null)
        {
            Debug.LogError("Grid data must be generated first");
            return;
        }

        ClearMap(); //Clear out previous map if there is one
        for(int x = 0; x < Grid.GetLength(0); x++) //Rows
        {
            for (int y = 0; y < Grid.GetLength(1); y++) //Columns
            {
                if(Grid[x, y] == null)
                {
                    Debug.LogError($"No grid tile created at: [{x},{y}]");
                    continue;
                }

                //Look for a prefab associated with the tile value
                GameObject prefab = _tilePrefabs[Grid[x, y].Value];
                if (prefab == null) continue; //If no prefab available, skip this tile

                //Instantiate the object and place it accordingly
                GameObject go = Instantiate(prefab, _tileRoot);
                go.name = $"Tile [{x},{y}]";
                go.transform.localPosition = GetTilePos(x, y);
                go.transform.localScale = Vector3.one * _scale;
            }
        }
    }

    //Clear out our previous map
    public void ClearMap()
    {
        if (_tileRoot == null)
        {
            Debug.LogError("No tile root transform found, please assign one");
            return;
        }

        for(int i = _tileRoot.childCount - 1; i >= 0; i--) //Iterate backwards through children and destroy them
        {
            DestroyImmediate(_tileRoot.GetChild(i).gameObject);
        }
    }

    public Vector3 GetTilePos(int x, int y)
    {
        //Determine instantiation pos
        Vector3 pos = Vector3.zero;
        pos.x = x * _scale;
        pos.z = y * _scale;

        //We want our root point to be in the center, so offset the tile by halfway points
        pos.x -= ((Grid.GetLength(0) - 1) / 2f) * _scale;
        pos.z -= ((Grid.GetLength(1) - 1) / 2f) * _scale;
        return pos;
    }
}
