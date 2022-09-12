using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField]
    private bool lockCursor = false;
    [SerializeField]
    private bool isController = false;
    [SerializeField]
    private bool inverseY = false;
    [SerializeField]
    private bool inverseX = false;

    [SerializeField]
    private float cameraSensitivity = 0.7f;

    [SerializeField]
    private float rotationSmoothTime = 0.12f;
    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;

    [SerializeField]
    private Transform target;
    [SerializeField]
    private float dstFromTarget = 2f;

    [SerializeField]
    private Vector2 pitchMinMax = new Vector2(-40, 85);


    private float yaw;
    private float pitch;

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        yaw = inverseX ? yaw - Input.GetAxis("Mouse X") * cameraSensitivity : yaw + Input.GetAxis("Mouse X") * cameraSensitivity;
        pitch = inverseY ? pitch + Input.GetAxis("Mouse Y") * cameraSensitivity : pitch - Input.GetAxis("Mouse Y") * cameraSensitivity;

        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * dstFromTarget;
    }
}
