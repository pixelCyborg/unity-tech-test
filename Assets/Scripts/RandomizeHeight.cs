using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeHeight : MonoBehaviour
{
    [SerializeField] private float _minHeight = 0.5f;
    [SerializeField] private float _maxHeight = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 scale = transform.localScale;
        scale.y = Random.Range(_minHeight, _maxHeight);
        transform.localScale = scale;
    }

}
