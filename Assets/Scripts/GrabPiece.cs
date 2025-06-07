using System.Collections;
using UnityEngine;

public class GrabPiece : MonoBehaviour
{
    public LayerMask blackPieceLayer;

    private GameObject selectedPiece = null;
    private Vector3 pieceOffset;
    private Vector3 originalSnappedPosition;

    void Update()
    {
        if (CheckersLogic.Instance.currentTurn != PlayerTurn.Black)
            return;

        Ray ray = GlobalRaycaster.globalRay;
        bool triggerPressed = GlobalRaycaster.isTriggerPressed;

        if (triggerPressed)
        {
            if (selectedPiece == null)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 40f, blackPieceLayer))
                {
                    selectedPiece = hit.collider.gameObject;

                    var boardCoords = CheckersLogic.Instance.WorldToBoard(selectedPiece.transform.position);
                    originalSnappedPosition = CheckersLogic.Instance.BoardToWorld(boardCoords.row, boardCoords.col);
                }
            }
            else
            {
                // Snap the piece to nearest board square based on current raycast
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var (row, col) = CheckersLogic.Instance.WorldToBoard(hit.point);
                    Vector3 snappedPos = CheckersLogic.Instance.BoardToWorld(row, col);
                    selectedPiece.transform.position = snappedPos;
                }
            }
        }
        else if (selectedPiece != null)
        {
            if (CheckersLogic.Instance.IsValidMove(originalSnappedPosition, selectedPiece.transform.position))
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