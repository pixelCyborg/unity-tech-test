using System;
using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [Header("Pathing")]
    [SerializeField]
    private NavGridPathNode[] _currentPath = Array.Empty<NavGridPathNode>();
    [SerializeField] private NavGrid _grid; //A reference to our navigation grid
    [SerializeField] private float _bezierAnchorWeight = 0.5f;

    [Header("Configuration")]
    [SerializeField] private float _speed = 10.0f;
    [SerializeField] private float _rotationSpeed = 10f;

    [SerializeField] [NaughtyAttributes.CurveRange(0,0,1,1)]
    private AnimationCurve _accelerationCurve; //Animation curve to help control our movement speed
    [SerializeField] private float _accelerationRate = 1f; //Rate at which we follow our curve
    private float _accelerationTime; //For keeping track of how long we've been accelerating

    [Header("Display Only")]
    [SerializeField] private int _currentPathIndex = 0;
    //Bezier Navigation
    [SerializeField] private float _bezierPoint = 0f;

    void Update()
    {
        // Check Input
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                //Adding this tidbit in since we are working in 2D space
                Vector3 hitPos = hitInfo.point;
                hitPos.y = transform.position.y;

                _currentPath = _grid.GetPath(transform.position, hitPos);
                _currentPathIndex = 0;
                _bezierPoint = 0f; //Reset the bezier point as well
            }
        }

        TraverseCurve();

        _accelerationTime += Time.deltaTime * _accelerationRate; //Keep track of how long we've been accelerating
        if (_currentPathIndex == _currentPath.Length - 1) _accelerationTime = 0f; //If we are not moving, reset acceleration time
    }
    //Bezier-Focused movement function
    private void TraverseCurve()
    {
        if (_currentPathIndex < _currentPath.Length)
        {
            //Get a list of valid point nodes
            List<Vector3> points = GetBezierPoints();

            //Get our current position on the bezier curve
            Vector3 targetPos  = Bezier(points, _bezierPoint);

            Vector3 targetDir = targetPos - transform.position;
            if (targetDir != Vector3.zero)
            {
                Quaternion lookDir = Quaternion.LookRotation(targetDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDir, _rotationSpeed * Time.deltaTime);
            }

            //Lerp the position for less jitter
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * _speed);

            //Progress our character along the bezier line
            float speedMulti = _speed * _accelerationCurve.Evaluate(_accelerationTime); //Use our curve property to smooth acceleration
            float length = EstimateLength(points); //To account for varying path lengths, adjust the speed based on distance
            if (length > 0) speedMulti = speedMulti / length; //Be sure not to divide by 0
            _bezierPoint += Time.deltaTime * speedMulti;

            //If our bezier is >1 we have moved on to the next node
            if (_bezierPoint > 1f)
            {
                _currentPathIndex ++; //Iterate the index
                _bezierPoint -= 1f; //Keep any remainder to keep progress smooth
            }
        }
    }

    private List<Vector3> GetBezierPoints ()
    {
        List<Vector3> points = new List<Vector3>();
        List<Vector3> anchors = new List<Vector3>();

        //Start Pos & Anchor
        Vector3 startPos = _currentPath[_currentPathIndex].Position;
        points.Add(startPos);
        if (_currentPathIndex > 0)
        {
            Vector3 startAnchor = startPos + (startPos - _currentPath[_currentPathIndex - 1].Position) * _bezierAnchorWeight;
            points.Add(startAnchor);
        }

        //End Pos & Anchor
        if (_currentPathIndex < _currentPath.Length - 1)
        {
            Vector3 endPos = _currentPath[_currentPathIndex + 1].Position;
            points.Add(endPos);
        }
        return points;
    }

    //Simple estimation for length of all points in a list
    private float EstimateLength(List<Vector3> points)
    {
        if (points == null || points.Count < 2) return 0f;
        float length = 0;
        for(int i = 0; i < points.Count - 1; i++)
        {
            length += Vector3.Distance(points[i], points[i + 1]);
        }
        return Vector3.Distance(points[0], points[points.Count - 1]);
    }

    //Recursively calculate our bezier. We don't need any of that fancy math, we have for loops!
    //1. Calculate point at t for each pair of points
    //2. If more than one result, calculate again for the resulting points
    //3. Repeat until only 1 point is left
    private Vector3 Bezier(List<Vector3> points, float t)
    {
        //Validate our point list
        if (points == null || points.Count == 0) return Vector3.zero;
        if (points.Count == 1) return points[0];

        //Cache for our calculated points
        List<Vector3> resultPoints = new List<Vector3>();
        //Calculate points at t for each current & next point (excluding last)
        for(int i = 0; i < points.Count - 1; i++)
        {
            resultPoints.Add(CalcBezierPoint(points[i], points[i + 1], t));
        }

        //Repeat if we have more than one point, otherwise we have our result
        if (resultPoints.Count > 1) return Bezier(resultPoints, t);
        else return resultPoints[0];
    }
    //Helper method for calculating Bezier value for each point in 2 vectors
    private Vector3 CalcBezierPoint(Vector3 a, Vector3 b, float t)
    {
        Debug.DrawLine(a, b, Color.cyan);
        return new Vector3(
                CalcBezierValue(a.x, b.x, t),
                CalcBezierValue(a.y, b.y, t),
                CalcBezierValue(a.z, b.z, t));
    }
    private float CalcBezierValue(float a, float b, float t)
    {
        return (1 - t) * a + (t * b);
    }

    //UNUSED =====================================
    //Original Movement Function
    private void TraversePath()
    {
        // Traverse
        if (_currentPathIndex < _currentPath.Length)
        {
            var currentNode = _currentPath[_currentPathIndex];

            var maxDistance = _speed * Time.deltaTime;
            var vectorToDestination = currentNode.Position - transform.position;
            var moveDistance = Mathf.Min(vectorToDestination.magnitude, maxDistance);

            var moveVector = vectorToDestination.normalized * moveDistance;
            moveVector.y = 0f; // Ignore Y
            transform.position += moveVector;

            if (transform.position == currentNode.Position)
                _currentPathIndex++;
        }
    }
}
