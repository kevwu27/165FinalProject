using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPiece : MonoBehaviour
{
    public Transform rightHandTransform;
    public LineRenderer lineRenderer;
    public float rayLength = 40f;
    public LayerMask pieceLayer;

    public GameObject board; // assign in Inspector
    private Bounds boardBounds;

    private GameObject selectedPiece = null;
    private Vector3 pieceOffset;
    private float lockedYHeight;

    void Start()
    {
        if (board != null)
        {
            Collider boardCol = board.GetComponent<Collider>();
            if (boardCol != null)
            {
                boardBounds = boardCol.bounds;
            }
        }
    }

    void Update()
    {
        Vector3 rayStart = rightHandTransform.position;
        Vector3 rayDir = rightHandTransform.forward;
        lineRenderer.SetPosition(0, rayStart);
        lineRenderer.SetPosition(1, rayStart + rayDir * rayLength);

        bool triggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (triggerPressed)
        {
            if (selectedPiece == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(rayStart, rayDir, out hit, rayLength, pieceLayer))
                {
                    selectedPiece = hit.collider.gameObject;
                    pieceOffset = selectedPiece.transform.position - hit.point;
                    lockedYHeight = selectedPiece.transform.position.y; // lock Y once
                }
            }
            else
            {
                RaycastHit hit;
                Vector3 targetPos;

                if (Physics.Raycast(rayStart, rayDir, out hit, rayLength))
                {
                    targetPos = hit.point + pieceOffset;
                }
                else
                {
                    targetPos = rayStart + rayDir * rayLength;
                }

                // Clamp X and Z to stay inside the board bounds
                targetPos.x = Mathf.Clamp(targetPos.x, boardBounds.min.x, boardBounds.max.x);
                targetPos.z = Mathf.Clamp(targetPos.z, boardBounds.min.z, boardBounds.max.z);
                targetPos.y = lockedYHeight; // keep Y steady — no bouncing

                selectedPiece.transform.position = targetPos;
            }
        }
        else
        {
            selectedPiece = null;
        }
    }
}
