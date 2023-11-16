using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// Given the current and desired location, return a path to the destination.
/// 
/// HC NOTES
/// =============
/// This class should solely be used for navigation purposes. We are going to get the data from our Map class
/// and then process it here for navigation
/// </summary>
/// 
public class NavGrid : MonoBehaviour
{
    #region STRUCTS
    //Simple struct to store our 2D map indexes
    public class Coord 
    {
        public int X;
        public int Y;
        public float Value;
        public Coord Previous;
        //public Coord Previous;


        public Coord(int x, int y, float value)
        {
            X = x;
            Y = y;
            Value = value;
        }
        public Coord(int x, int y)
        {
            X = x;
            Y = y;
            Value = 0;
        }

        //Get Property so we can easily calculate Vector2.Distance
        public Vector2 Vector
        {
            get { return new Vector2(X, Y); }
        }

        //Override default comparers, class is equal if coordinates are the same, other fields don't matter for comparison
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1?.X == c2?.X && c1?.Y == c2?.Y;
        }
        public static bool operator !=(Coord c1, Coord c2)
        {
            return c1?.X != c2?.X && c1?.Y != c2?.Y;
        }
        public override bool Equals(object obj)
        {
            return obj is Coord coord &&
                   X == coord?.X && Y == coord?.Y;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
    #endregion

    [Header("Configuration")]
    [SerializeField] private float _timeStep = 0.1f;

    [Header("References")]
    [SerializeField] private Map _map;

    public NavGridPathNode[] GetPath(Vector3 origin, Vector3 destination)
    {
        //Convert world points to coordinates
        Coord start = _map.GetClosestCoordinates(origin);
        Coord finish = _map.GetClosestCoordinates(destination);

        //Get our coordinates from the path and convert them to nodes
        Coord[] coords = CalculatePath(start, finish);
        NavGridPathNode[] nodes = new NavGridPathNode[coords.Length];

        nodes[0] = new NavGridPathNode() { Position = origin }; //Use the original position for 1st node to avoid snapping to tiles mid-movement
        for (int i = 1; i < coords.Length; i++)
        {
            //Convert our coord nodes into path nodes                     Always use user pos for height
            nodes[i] = new NavGridPathNode() { Position = _map.GetTilePos(coords[i], origin.y) };
        }

        return nodes;
    }

    //G Cost = Distance from starting node
    //H Cost = Distance from end node
    //F Cost = G + H Cost;

    //Basic Algorithm:
    //1. Calculate F Cost from nodes sorrounding origin
    //2. Repeat above for lowest F Node cost.
    //3. Rinse and repeat, ignoring paths that lead us to walls
    //3a. If we find a lower FCost while calculating neighbors, replace that in our algorithm history
    private Coord[] CalculatePath(Coord origin, Coord destination)
    {
        //Create our cache of calculated nodes. Somewhat redundant now that value is stored in coord
        Dictionary<Vector2, float> costs = new Dictionary<Vector2, float>();
        //The border list is our buffer for tiles we want to check. Treat this as a jank priority queue
        //Since Unity's C# version doesn't have one
        //To make this more efficient we could implement our own priority queue
        List<Coord> border = new List<Coord>();
        Coord current = null;
        //A simple flag to tell us if we succesfully reached the goal.
        bool _destinationReached = false;
        
        //Add our initial tile to the border
        origin.Value = GetFCost(origin, origin, destination);
        costs.Add(origin.Vector, origin.Value); //Cache the cost
        border.Insert(0, (origin)); //Insert at beginning

        //Start iterating through neighbors
        while (border.Count > 0)
        {
            current = border[0];
            border.RemoveAt(0); //Dequeue

            //If we have arrived at our destination, end the loop early
            if (current == destination)
            {
                _destinationReached = true;
                break;
            }

            Coord[] neighbors = GetNeighbors(current.X, current.Y);
            if (neighbors.Length == 0) continue; //If there is no viable spot to go, move on to next border

            //Check all walkable neighbors of current tile
            foreach (Coord neighbor in neighbors)
            {
                //Calculate our cost if it is a movable area. Otherwise cost is -1
                float cost = GetFCost(origin, neighbor, destination);
                Vector2 neighborVector = neighbor.Vector;

                //Cache the cost if we haven't cached a more effective value already
                //This means we should add this neighbor to our border list
                if(!costs.ContainsKey(neighborVector) || costs[neighborVector] > cost)
                {
                    //Place tile in queue, ordered by ascending value
                    int index = 0;
                    while (index < border.Count && cost > border[index].Value)
                        index++;

                    //Store the cost and link the neighbor tile back to our current
                    neighbor.Value = cost;
                    neighbor.Previous = current;

                    costs.Add(neighborVector, cost); //Add this to our cost dictionary
                    border.Insert(index, neighbor); //Insert a new coordinate with the recorded value

                    Debug.DrawLine(_map.GetTilePos(neighbor), _map.GetTilePos(current), Color.cyan, 0.2f);
                }
            }
        }

        //If we reached the destination, build a round out of linked "Previous" nodes
        //Retroactively check our "Previous" nodes until we have a full path
        //Use a stack so that once we reach the beginning, that node will be at index 0
        Stack<Coord> route = new Stack<Coord>();
        if (_destinationReached && current != null)
        {
            //Add the first node
            route.Push(current);
            //Then iterate through previous nodes until none are left
            while (current.Previous != null)
            {
                current = current.Previous;
                route.Push(current);
            }
        }
        else //If we fail to make it to our goal, return our origin node
        {
            Debug.LogError("No route available");
            route.Push(origin);
        }
        return route.ToArray();
    }

    //A simple function to get a list of neighboring indexes
    //To expand on this we could add stricter rules like corner tiles being blocked off by adjacent walls
    //Or not getting diagonal tiles entirely
    private Coord[] GetNeighbors(int origX, int origY)
    {
        //Check neighbors. Since this is grid-based there are 8 directions to check
        List<Coord> neighbors = new List<Coord>();
        for(int x = origX - 1; x <= origX + 1; x++)
        {
            //Check for out of bounds X
            if (x < 0 || x >= _map.Grid.GetLength(0)) continue;
            for (int y = origY - 1; y <= origY + 1; y++)
            {
                //Check for out of bounds Y
                if (y < 0 || y >= _map.Grid.GetLength(1)) continue;
                //Also skip if this is the origin
                if (origX == x && origY == y) continue;
                //If this tile is not walkable, skip
                if (!_map.Grid[x, y].Walkable) continue;
                //Add to list of possible neighbors if all conditions are met
                neighbors.Add(new Coord(x, y));
            }
        }

        return neighbors.ToArray();
    }

    //Calculate the total cost of this node using the built in Vector2.Distance function
    private float GetFCost(Coord origin, Coord target, Coord destination)
    {
        float gCost = Vector2.Distance(origin.Vector, target.Vector);
        float hCost = Vector2.Distance(target.Vector, destination.Vector);
        return gCost + hCost;
    }
}
