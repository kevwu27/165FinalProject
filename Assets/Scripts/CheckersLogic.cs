using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    Empty = 0,
    Black = 1,
    BlackKing = 2,
    White = -1,
    WhiteKing = -2
}
public class CheckersLogic : MonoBehaviour
{
    public static CheckersLogic Instance;
    private Vector3 boardMin;
    private float cellWidth;
    private float cellDepth;

    public List<GameObject> whitePieces;   
    public List<GameObject> blackPieces; 
    public GameObject board;
    
    private void Awake()
    {
        Instance = this;
    }

    public PieceType[,] boardState = new PieceType[8, 8];

    public float boardEdgeMarginPercent = 0.01f;
    private Bounds boardBounds;
    private float lockedYHeight;
    void Start()
    {
        Collider boardCol = board.GetComponent<Collider>();
        if (boardCol != null)
        {
            boardBounds = boardCol.bounds;
            cellWidth = (boardBounds.size.x * (1 - 2 * boardEdgeMarginPercent)) / 8f;
            cellDepth = (boardBounds.size.z * (1 - 2 * boardEdgeMarginPercent)) / 8f;
            lockedYHeight = boardCol.bounds.center.y + boardCol.bounds.extents.y +0.05f;
            boardMin = new Vector3(
                boardBounds.min.x + boardBounds.size.x * boardEdgeMarginPercent,
                lockedYHeight,
                boardBounds.min.z + boardBounds.size.z * boardEdgeMarginPercent
            );
        }
        SpawnPieces();
    }
    void SpawnPieces()
    {
        int whiteIndex = 0;
        int blackIndex = 0;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 1)
                {
                    Vector3 worldPos = CheckersLogic.Instance.BoardToWorld(row, col);

                    if (row <= 2 && blackIndex < blackPieces.Count)
                    {
                        GameObject piece = blackPieces[blackIndex++];
                        piece.transform.position = worldPos;
                        boardState[row, col] = PieceType.Black;
                    }
                    else if (row >= 5 && whiteIndex < whitePieces.Count)
                    {
                        GameObject piece = whitePieces[whiteIndex++];
                        piece.transform.position = worldPos;
                        boardState[row, col] = PieceType.White;
                    }
                }
            }
        }
        Debug.Log("Spawn complete.");
    }

    public (int row, int col) WorldToBoard(Vector3 worldPos)
    {
        int col = Mathf.Clamp(Mathf.FloorToInt((worldPos.x - boardMin.x) / cellWidth), 0, 7);
        int row = Mathf.Clamp(Mathf.FloorToInt((worldPos.z - boardMin.z) / cellDepth), 0, 7);

        return (row, col);
    }

    public Vector3 BoardToWorld(int row, int col)
    {
        float x = boardMin.x + col * cellWidth + cellWidth / 2f;
        float z = boardMin.z + row * cellDepth + cellDepth / 2f;

        return new Vector3(x, lockedYHeight, z);
    }

    public bool IsValidMove(Vector3 fromWorldPos, Vector3 toWorldPos)
    {
        var (fromRow, fromCol) = WorldToBoard(fromWorldPos);
        var (toRow, toCol) = WorldToBoard(toWorldPos);

        PieceType movingPiece = boardState[fromRow, fromCol];
        if (movingPiece == PieceType.Empty) return false;
        if (boardState[toRow, toCol] != PieceType.Empty) return false;
        int dir = (int)Mathf.Sign((int)movingPiece);

        // Allow diagonal forward movement by 1 for normal pieces
        bool isKing = movingPiece == PieceType.WhiteKing || movingPiece == PieceType.BlackKing;

        if (isKing)
        {
            if (Mathf.Abs(toRow - fromRow) == 1 && Mathf.Abs(toCol - fromCol) == 1)
                return true;
        }
        else
        {
            if (toRow == fromRow + dir && Mathf.Abs(toCol - fromCol) == 1)
                return true;
        }
        return false;
    }

    public void ApplyMove(Vector3 fromWorldPos, Vector3 toWorldPos)
    {
        var (fromRow, fromCol) = WorldToBoard(fromWorldPos);
        var (toRow, toCol) = WorldToBoard(toWorldPos);

        boardState[toRow, toCol] = boardState[fromRow, fromCol];
        boardState[fromRow, fromCol] = PieceType.Empty;
        Debug.Log($"Moved piece from ({fromRow}, {fromCol}) to ({toRow}, {toCol})");
        PrintBoard();
    }
    private void PrintBoard()
    {
        string boardStr = "";
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int val = (int)boardState[row, col];
                boardStr += $"{val,3}";
            }
            boardStr += "\n";
        }
        Debug.Log("Board State:\n" + boardStr);
    }
}
