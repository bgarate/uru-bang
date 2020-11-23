using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Camera Camera;

    public float RotationSpeed = 100;

    private float xRotation = 90;
    
    public float zoomOutFov = 60;
    public float zoomInFov = 20;
    public float zoomSpeed = 10;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        bool zoomIn = Input.GetButton("Fire2");

        if (zoomIn)
            Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, zoomInFov, Time.deltaTime * zoomSpeed);
        else
            Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, zoomOutFov, Time.deltaTime * zoomSpeed);
        
        var x = Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Mouse Y");

        transform.Rotate(transform.up, x * RotationSpeed * Time.deltaTime);

        xRotation -= y * RotationSpeed * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        Camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}