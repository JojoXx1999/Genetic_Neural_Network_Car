using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to make the camera follow the current individual
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    public float offset, YOffset, rotate;
    public float x;

    // Update is called once per frame
    void Update()
    {
        //Set camera position and rotation to be looking at the cube
        Vector3 setPosition = target.transform.position - transform.forward * offset;
        Camera.main.transform.position = Camera.main.transform.position * x + setPosition * (1f - x);

        //Look at the target (cube)
        Camera.main.transform.LookAt(target.transform.position);
        Camera.main.transform.Rotate(rotate, 0, 0);

        //Move up on Y-axis so it's not in the ground
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, YOffset, Camera.main.transform.position.z);
    }
}
