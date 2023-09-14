using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    private NavGridPathNode[] _currentPath = Array.Empty<NavGridPathNode>();
    private int _currentPathIndex = 0;
    
    [SerializeField]
    private NavGrid _grid;
    [SerializeField]
    private float _speed = 10.0f;

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                _currentPath = _grid.GetPath(transform.position, hitInfo.point);
                _currentPathIndex = 0;
            }
        }

        if (_currentPathIndex < _currentPath.Length)
        {
            var currentNode = _currentPath[_currentPathIndex];
            
            var maxDistance = _speed * Time.deltaTime;
            var vectorToDestination = currentNode.Position - transform.position;
            var moveDistance = Mathf.Min(vectorToDestination.magnitude, maxDistance);
            
            transform.position += vectorToDestination.normalized * moveDistance;

            if (transform.position == currentNode.Position)
                _currentPathIndex++;
        }
    }
}
