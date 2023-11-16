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
    [SerializeField] private NavGrid _floor; //Floor should be a standard Unity plane mesh
    [SerializeField] private Player _player;

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
                go.transform.position = GetTilePos(x, y, _tileRoot.position.y);
                go.transform.localScale = new Vector3(1f * _scale, 1f, 1f * _scale);
            }
        }

        if (_floor != null)
        {
            Vector3 floorScale = Vector3.one;
            floorScale.x = _width * _scale * 0.1f;
            floorScale.z = _height * _scale * 0.1f;
            _floor.transform.localScale = floorScale;
        }

        //Randomize player pos
        PlacePlayerInEmptySpace();
    }

    //Randomly select tiles until finding an empty one, 
    private void PlacePlayerInEmptySpace()
    {
        if (_player == null) return;
        if (Grid == null) return;

        int failsafe = 99; //Use a limited number of trys in case there are no suitable positions
        int x = Random.Range(0, Grid.GetLength(0));
        int y = Random.Range(0, Grid.GetLength(1));

        while(!Grid[x,y].Walkable && failsafe > 0)
        {
            x = Random.Range(0, Grid.GetLength(0));
            y = Random.Range(0, Grid.GetLength(1));
            failsafe--;
        }

        _player.transform.position = GetTilePos(x, y, _player.transform.position.y);
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

    //Methods for converting coordinates to world space
    public Vector3 GetTilePos(NavGrid.Coord coord, float height = 0f)
    {
        return GetTilePos(coord.X, coord.Y);
    }
    public Vector3 GetTilePos(int x, int y, float height = 0f)
    {
        //Determine instantiation pos
        Vector3 pos = Vector3.zero;
        pos.x = x * _scale;
        pos.z = y * _scale;

        //We want our root point to be in the center, so offset the tile by halfway points
        pos.x -= ((Grid.GetLength(0) - 1) / 2f) * _scale;
        pos.z -= ((Grid.GetLength(1) - 1) / 2f) * _scale;

        //Add root position so we get the world pos, and adjust for height
        pos = _tileRoot.position + pos;
        pos.y = height;

        return pos;
    }

    //And vice versa, we can use this to get coordinates from click positions
    public NavGrid.Coord GetClosestCoordinates(Vector3 worldPos)
    {
        //Get the local position on the grid
        Vector3 localPos = _tileRoot.InverseTransformPoint(worldPos);
        //Adjust positions to account for center anchor
        localPos.x += ((Grid.GetLength(0) - 1) / 2f) * _scale;
        localPos.z += ((Grid.GetLength(1) - 1) / 2f) * _scale;

        //Build our coordinates from local position / scale
        NavGrid.Coord coord = new NavGrid.Coord(
            Mathf.RoundToInt(localPos.x / _scale),
            Mathf.RoundToInt(localPos.z / _scale));

        return coord;
    }
}
