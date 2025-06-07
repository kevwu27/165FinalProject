using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuFollowCamera : MonoBehaviour
{
    public Transform cameraTransform; // Drag in OVRCameraRig/CenterEyeAnchor
    public float followDistance = 4f;
    public float heightOffset = 0f;
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Target position in front of camera
        Vector3 targetPos = cameraTransform.position + cameraTransform.forward * followDistance;
        targetPos.y += heightOffset;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        // Always face the camera (flat UI)
        Vector3 lookDir = transform.position - cameraTransform.position;
        lookDir.y = 0; // Optional: lock vertical rotation
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
