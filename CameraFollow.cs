using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    /* public Transform target;

    public float smoothSpeed = 7f;
    private Vector3 offset = new Vector3(-0.4f, 0.5f, -1.65f);
    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothPosition;
    } */

    // public string playerName;

    public bool lockCursor;

    public float mouseSensitivity = 5f;
    public Transform target;
    public float distanceFromTarget = 2f;

    public Vector2 pitchMinMax = new Vector2(-17, 70);

    public float rotationSmoothTime = 0.18f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    private bool isSticked = false;

    float yaw, pitch;
    private void Start()
    {
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!isSticked) target = GameObject.Find(Player.Count.ToString() + "/CameraFollowPoint").GetComponentInChildren<Transform>(); isSticked = true;
    }
    private void LateUpdate()
    {
        if (!isSticked)
            return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);

        // Vector3 targetRotation = new Vector3(pitch, yaw);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * distanceFromTarget;
    }
}
