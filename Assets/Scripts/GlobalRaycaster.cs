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

    private OVRInput.Controller lastController;

    void Update()
    {
        // Check active controller
        var currentController = OVRInput.GetActiveController();

        // Hide ray if controller is inactive (e.g. switched to hands or unknown)
        if ((currentController & OVRInput.Controller.RTouch) == 0)
        {
            lineRenderer.enabled = false;
            return;
        }

        // Otherwise, show and update the ray
        lineRenderer.enabled = true;

        Vector3 rayStart = rightHandTransform.position;
        Vector3 rayDir = rightHandTransform.forward;

        globalRay = new Ray(rayStart, rayDir);
        isTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        lineRenderer.SetPosition(0, rayStart);
        lineRenderer.SetPosition(1, rayStart + rayDir * rayLength);
    }
}
