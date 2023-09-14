using UnityEngine;

public class Player : MonoBehaviour
{
    public NavGrid Grid;
    
    void Start()
    {
        Grid.Initialize();        
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                transform.position = Grid.GetPath(transform.position, hitInfo.point)[^1].Position;
            }
        }
    }
}
