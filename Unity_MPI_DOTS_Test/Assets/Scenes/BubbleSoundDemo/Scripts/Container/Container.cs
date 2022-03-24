using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public float height;
    public float maxDepth;
    public float currentDepth;
    public float fillRate;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddWater(int water)
    {
        currentDepth += water * fillRate;
    }
    public float GenerateRandomDepth()
    {
        return Random.Range(0,currentDepth);
    }
}
