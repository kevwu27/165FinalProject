using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float rotateSpeed = 60f;
    public Transform rigRoot;  // Main camera from OVRCameraRig

    void Update()
    {
        // LEFT JOYSTICK MOVEMENT (assumes primary hand = left)
        Vector2 moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = Camera.main.transform.TransformDirection(move); // move relative to headset view
        move.y = 0; // lock to ground
        rigRoot.position += move * moveSpeed * Time.deltaTime;

        // RIGHT JOYSTICK ROTATION
        float rotateInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;
        if (Mathf.Abs(rotateInput) > 0.1f) // add deadzone
        {
            rigRoot.Rotate(Vector3.up, rotateInput * rotateSpeed * Time.deltaTime);
        }
    }
}
