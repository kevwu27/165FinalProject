using System.Collections;
using UnityEngine;

public class GrabPiece : MonoBehaviour
{
    public LayerMask blackPieceLayer;
    public AudioSource grabAudio;
    public AudioSource placeAudio;

    private GameObject selectedPiece = null;
    private GameObject currentlyHovered = null;
    private Vector3 originalSnappedPosition;

    void Update()
    {
        if (CheckersLogic.Instance.currentTurn != PlayerTurn.Black)
            return;

        Ray ray = GlobalRaycaster.globalRay;
        bool triggerPressed = GlobalRaycaster.isTriggerPressed;

        // Hover highlight
        if (selectedPiece == null)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 40f, blackPieceLayer))
            {
                GameObject hitPiece = hit.collider.gameObject;

                if (currentlyHovered != hitPiece)
                {
                    if (currentlyHovered != null)
                        currentlyHovered.GetComponent<HoverHighlight>()?.SetHighlight(false);

                    currentlyHovered = hitPiece;
                    currentlyHovered.GetComponent<HoverHighlight>()?.SetHighlight(true);
                }
            }
            else if (currentlyHovered != null)
            {
                currentlyHovered.GetComponent<HoverHighlight>()?.SetHighlight(false);
                currentlyHovered = null;
            }
        }

        // Handle selection
        if (triggerPressed)
        {
            if (selectedPiece == null && currentlyHovered != null)
            {
                selectedPiece = currentlyHovered;
                var boardCoords = CheckersLogic.Instance.WorldToBoard(selectedPiece.transform.position);
                originalSnappedPosition = CheckersLogic.Instance.BoardToWorld(boardCoords.row, boardCoords.col);

                grabAudio?.Play();  // Play grab sound
            }
            else if (selectedPiece != null)
            {
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
                placeAudio?.Play();  // Play place sound
                CheckersLogic.Instance.currentTurn = PlayerTurn.White;
                StartCoroutine(CheckersLogic.Instance.MakeAIMove());
            }
            else
            {
                selectedPiece.transform.position = originalSnappedPosition;
            }

            selectedPiece.GetComponent<HoverHighlight>()?.SetHighlight(false);
            selectedPiece = null;
        }
    }
}
