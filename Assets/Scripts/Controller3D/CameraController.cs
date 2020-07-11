using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float rotationSpeed;
    [SerializeField]
    float moveSpeed;
    [SerializeField]
    float zoomSpeed;

    // Update is called once per frame
    void Update()
    {
        //rotation
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up * rotationSpeed);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.down * rotationSpeed);
        }

        //movement
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * moveSpeed);
        transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * moveSpeed);

        //zoom
        transform.Translate((Vector3.forward + transform.up * -1) * Input.mouseScrollDelta.y * zoomSpeed);
    }
}