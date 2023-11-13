using UnityEngine;

public class NavGrid : MonoBehaviour
{
    /// <summary>
    /// Given the current and desired location, return a path to the destination.
    /// 
    /// HC NOTES
    /// =============
    /// This class should solely be used for navigation purposes. We are going to get the data from our Map class
    /// and then process it here for navigation
    /// </summary>
    public NavGridPathNode[] GetPath(Vector3 origin, Vector3 destination)
    {
        return new NavGridPathNode[]
        {
            new() { Position = origin },
            new() { Position = destination }
        };
    }
}
