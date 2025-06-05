using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public Transform cameraTransform;  // Main camera from OVRCameraRig

    void Update()
    {
        // Get joystick input (left thumbstick)
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // Move in the direction the headset is facing
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Flatten to horizontal plane
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Combine direction
        Vector3 moveDir = forward * input.y + right * input.x;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
