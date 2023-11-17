using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple Camera Controls
public class CameraControls : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private float _rotateDecelleration;
    [SerializeField] private float _zoomSpeed;
    [SerializeField] private float _zoomDecelleration;

    private float _currentRotateSpeed;
    private float _currentZoomSpeed;

    // Update is called once per frame
    void Update()
    {
        //Middle mouse button held down
        if(Input.GetMouseButton(2))
        {
            float x = Input.GetAxis("Mouse X");
            _currentRotateSpeed += x * _rotateSpeed;
        }

        transform.Rotate(Vector3.up * _currentRotateSpeed * Time.deltaTime, Space.World);
        _currentRotateSpeed = Mathf.Lerp(_currentRotateSpeed, 0f, _rotateDecelleration * Time.deltaTime);

        _currentZoomSpeed += Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed;
        _cam.orthographicSize -= _currentZoomSpeed * Time.deltaTime;
        _currentZoomSpeed = Mathf.Lerp(_currentZoomSpeed, 0, _zoomDecelleration * Time.deltaTime);
    }
}
