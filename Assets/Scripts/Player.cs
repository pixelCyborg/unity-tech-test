using UnityEngine;

public class Player : MonoBehaviour
{
    public NavGrid Grid;
    public Vector3 Destination;
    public float Speed = 10.0f;
    
    void Start()
    {
        Destination = transform.position;
        Grid.Initialize();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                Destination = Grid.GetPath(transform.position, hitInfo.point)[^1].Position;
            }
        }

        if (transform.position != Destination)
        {
            var maxDistance = Speed * Time.deltaTime;
            var vectorToDestination = Destination - transform.position;
            var moveDistance = Mathf.Min(vectorToDestination.magnitude, maxDistance);
            
            transform.position += vectorToDestination.normalized * moveDistance;
        }
    }
}
