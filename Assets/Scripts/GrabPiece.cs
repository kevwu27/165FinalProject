using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPiece : MonoBehaviour
{
    public Transform rightHandTransform;
    public LineRenderer lineRenderer;
    public float rayLength = 40f;
    public LayerMask blackPieceLayer;


    private GameObject selectedPiece = null;
    private Vector3 pieceOffset;
    private Vector3 originalSnappedPosition;

    void Start()
    {
    }

    void Update()
    {
        if (CheckersLogic.Instance.currentTurn != PlayerTurn.Black)
            return;

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
                if (Physics.Raycast(rayStart, rayDir, out hit, rayLength, blackPieceLayer))
                {
                    selectedPiece = hit.collider.gameObject;
                    pieceOffset = selectedPiece.transform.position - hit.point;
                    originalSnappedPosition = CheckersLogic.Instance.BoardToWorld(
                        CheckersLogic.Instance.WorldToBoard(selectedPiece.transform.position).row,
                        CheckersLogic.Instance.WorldToBoard(selectedPiece.transform.position).col
                    );
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
                var (row, col) = CheckersLogic.Instance.WorldToBoard(targetPos);
                selectedPiece.transform.position = CheckersLogic.Instance.BoardToWorld(row, col);
            }
        }
        else if (selectedPiece != null)
        {
            if(CheckersLogic.Instance.IsValidMove(originalSnappedPosition, selectedPiece.transform.position))
            {
                CheckersLogic.Instance.ApplyMove(originalSnappedPosition, selectedPiece.transform.position);

                CheckersLogic.Instance.currentTurn = PlayerTurn.White;
                StartCoroutine(CheckersLogic.Instance.MakeAIMove());
            }
            else
            {
                selectedPiece.transform.position = originalSnappedPosition;
            }
            selectedPiece = null;
        }
    }
}
