using UnityEngine;

public class NavGrid : MonoBehaviour
{
    public void Initialize()
    {
        
    }
    
    /// <summary>
    /// Given the current and desired location, return a path to the destination
    /// </summary>
    public NavGridPathSegment[] GetPath(Vector3 origin, Vector3 destination)
    {
        return new NavGridPathSegment[]
        {
            new() { Position = origin },
            new() { Position = destination }
        };
    }
}
