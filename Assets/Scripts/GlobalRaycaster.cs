using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GlobalRaycaster : MonoBehaviour
{
    public Transform rightHandTransform;
    public LineRenderer lineRenderer;
    public float rayLength = 40f;
    public LayerMask interactableLayers;

    public static Ray globalRay;
    public static bool isTriggerPressed;

    void Update()
    {
        Vector3 rayStart = rightHandTransform.position;
        Vector3 rayDir = rightHandTransform.forward;

        globalRay = new Ray(rayStart, rayDir);
        isTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        // Draw the visual ray
        lineRenderer.SetPosition(0, rayStart);
        lineRenderer.SetPosition(1, rayStart + rayDir * rayLength);
    }
}