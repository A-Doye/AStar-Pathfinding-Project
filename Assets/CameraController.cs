using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // The horizontal and verticle vectors of movement, used to ensure consistency of motion
    float horizontal = 0;
    float verticle = 0;
    // Move padding is the area in which the mouse can exist for the camera to start moving
    int movePadding = 10;
    // Speeds at which the camera will move and zoom
    private float panSpeed = 0.05f;
    private float scrollSpeed = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PanCamera();
        ZoomCamera();
    }

    // Moves the camera on the X and Z planes (by moving its parent camera target object)
    private void PanCamera()
    {
        verticle = 0;
        horizontal = 0;

        if (Input.GetKey("w"))// || Input.mousePosition.y >= Screen.height - movePadding)
        {
            verticle -= 1;
        }
        if (Input.GetKey("s"))// || Input.mousePosition.y <= 0 + movePadding)
        {
            verticle += 1;
        }

        if (Input.GetKey("d"))// || Input.mousePosition.x >= Screen.width - movePadding)
        {
            horizontal += 1;
        }
        if (Input.GetKey("a"))// || Input.mousePosition.x <= 0 + movePadding)
        {
            horizontal -= 1;
        }

        // Finds movement angle. 45 degrees are added as the camera is isometric
        float moveAngle = Mathf.Atan2(verticle, horizontal) + 0.25f * Mathf.PI;

        // Mathf.Atan2 returns -180 to 180, this converts it to 0 to 360
        if (moveAngle < 0f)
        {
            moveAngle += 2 * Mathf.PI;
        }

        // If the camera is being told to move, it is moved on the X and Z planes at the desired angle
        if (verticle != 0 || horizontal != 0)
        {
            transform.position += new Vector3(lengthdir_x(panSpeed, moveAngle), 0, lengthdir_z(panSpeed, moveAngle));
        }
    }
    // Zooms the camera using .orthographicSize, not by changing its location
    private void ZoomCamera()
    {
        Camera.main.orthographicSize -= Input.mouseScrollDelta.y;

        if (Camera.main.orthographicSize < 1)
        {
            Camera.main.orthographicSize = 1;
        }

        if (Camera.main.orthographicSize > 10)
        {
            Camera.main.orthographicSize = 10;
        }
    }

    // Returns X movement based on movement angle and speed
    private float lengthdir_x(float len, float dir)
    {
        return Mathf.Cos(dir) * len;
    }
    // Returns Z movement based on movement angle and speed
    private float lengthdir_z(float len, float dir)
    {
        return -Mathf.Sin(dir) * len;
    }

}