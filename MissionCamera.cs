using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleManualMovement(Time.deltaTime);
    }

    void HandleManualMovement(float deltaTime)
    {
        float speed = 40f;
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");
        float zoomAxis = Input.GetAxis("Mouse ScrollWheel");
        //if (hAxis == 0.0f && vAxis == 0.0f)     return;
        Vector3 movement = new Vector3();
        if (hAxis != 0.0f || vAxis != 0.0f) movement = new Vector3(hAxis, 0, vAxis) * speed * deltaTime;
        if (zoomAxis != 0.0f) movement += transform.TransformDirection(Vector3.forward) * 5000f * zoomAxis * deltaTime;

        transform.position += movement;

    }
}
