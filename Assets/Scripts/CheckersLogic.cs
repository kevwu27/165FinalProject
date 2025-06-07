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
public enum PlayerTurn
{
    Black,  // Human
    White   // AI
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

    public PlayerTurn currentTurn = PlayerTurn.Black;
    private Dictionary<GameObject, (int row, int col)> piecePositions = new();

    public GameObject kingVisualPrefab;

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
        piecePositions.Clear();
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
                        piecePositions[piece] = (row, col);
                    }
                    else if (row >= 5 && whiteIndex < whitePieces.Count)
                    {
                        GameObject piece = whitePieces[whiteIndex++];
                        piece.transform.position = worldPos;
                        boardState[row, col] = PieceType.White;
                        piecePositions[piece] = (row, col);
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

        int rowDelta = toRow - fromRow;
        int colDelta = toCol - fromCol;

        // Normal move
        if (Mathf.Abs(rowDelta) == 1 && Mathf.Abs(colDelta) == 1)
        {
            if (isKing || rowDelta == dir)
                return true;
        }

        // Capture move
        if (Mathf.Abs(rowDelta) == 2 && Mathf.Abs(colDelta) == 2)
        {
            int jumpedRow = fromRow + rowDelta / 2;
            int jumpedCol = fromCol + colDelta / 2;
            PieceType jumpedPiece = boardState[jumpedRow, jumpedCol];

            if (jumpedPiece != PieceType.Empty &&
                Mathf.Sign((int)jumpedPiece) != Mathf.Sign((int)movingPiece))
            {
                return true;
            }
        }

        return false;

        // if (isKing)
        // {
        //     if (Mathf.Abs(toRow - fromRow) == 1 && Mathf.Abs(toCol - fromCol) == 1)
        //         return true;
        // }
        // else
        // {
        //     if (toRow == fromRow + dir && Mathf.Abs(toCol - fromCol) == 1)
        //         return true;
        // }
        // return false;
    }

    public void ApplyMove(Vector3 fromWorldPos, Vector3 toWorldPos)
    {
        var (fromRow, fromCol) = WorldToBoard(fromWorldPos);
        var (toRow, toCol) = WorldToBoard(toWorldPos);
        PieceType movingPiece = boardState[fromRow, fromCol];

        boardState[toRow, toCol] = movingPiece;
        boardState[fromRow, fromCol] = PieceType.Empty;

        List<GameObject> pieceList = movingPiece > 0 ? blackPieces : whitePieces;
        GameObject movingGO = null;

        foreach (var piece in pieceList)
        {
            if (piecePositions.TryGetValue(piece, out var pos) && pos == (fromRow, fromCol))
            {
                movingGO = piece;
                break;
            }
        }

        if (movingGO != null)
        {
            movingGO.transform.position = BoardToWorld(toRow, toCol);
            piecePositions[movingGO] = (toRow, toCol);
        }

        // Handle capture
        if (Mathf.Abs(toRow - fromRow) == 2 && Mathf.Abs(toCol - fromCol) == 2)
        {
            int jumpedRow = (fromRow + toRow) / 2;
            int jumpedCol = (fromCol + toCol) / 2;
            PieceType jumpedPiece = boardState[jumpedRow, jumpedCol];
            boardState[jumpedRow, jumpedCol] = PieceType.Empty;

            List<GameObject> enemyList = jumpedPiece > 0 ? blackPieces : whitePieces;

            for (int i = 0; i < enemyList.Count; i++)
            {
                GameObject enemy = enemyList[i];
                if (piecePositions.TryGetValue(enemy, out var pos) && pos == (jumpedRow, jumpedCol))
                {
                    piecePositions.Remove(enemy);
                    Destroy(enemy);
                    enemyList.RemoveAt(i);
                    break;
                }
            }
        }

        // Handle king promotion
        if (movingPiece == PieceType.Black && toRow == 7)
        {
            boardState[toRow, toCol] = PieceType.BlackKing;
            PromoteToKing(movingGO);
        }
        else if (movingPiece == PieceType.White && toRow == 0)
        {
            boardState[toRow, toCol] = PieceType.WhiteKing;
            PromoteToKing(movingGO);
        }

        Debug.Log($"Moved piece from ({fromRow}, {fromCol}) to ({toRow}, {toCol})");
        PrintBoard();
    }


    // public void ApplyMove(Vector3 fromWorldPos, Vector3 toWorldPos)
    // {
    //     var (fromRow, fromCol) = WorldToBoard(fromWorldPos);
    //     var (toRow, toCol) = WorldToBoard(toWorldPos);
    //     PieceType movingPiece = boardState[fromRow, fromCol];

    //     boardState[toRow, toCol] = movingPiece;
    //     boardState[fromRow, fromCol] = PieceType.Empty;

    //     GameObject movedPiece = null;
    //     List<GameObject> pieceList = movingPiece > 0 ? blackPieces : whitePieces;

    //     foreach (var piece in pieceList)
    //     {
    //         var (r, c) = WorldToBoard(piece.transform.position);
    //         if (r == fromRow && c == fromCol)
    //         {
    //             piece.transform.position = BoardToWorld(toRow, toCol);
    //             movedPiece = piece;
    //             break;
    //         }
    //     }

    //      // Handle capture
    //     if (Mathf.Abs(toRow - fromRow) == 2 && Mathf.Abs(toCol - fromCol) == 2)
    //     {
    //         int jumpedRow = (fromRow + toRow) / 2;
    //         int jumpedCol = (fromCol + toCol) / 2;
    //         PieceType captured = boardState[jumpedRow, jumpedCol];

    //         if (captured != PieceType.Empty)
    //         {
    //             // Remove visual piece
    //             List<GameObject> list = captured > 0 ? blackPieces : whitePieces;
    //             foreach (var piece in list)
    //             {
    //                 var (r, c) = WorldToBoard(piece.transform.position);
    //                 if (r == jumpedRow && c == jumpedCol)
    //                 {
    //                     Destroy(piece);
    //                     list.Remove(piece);
    //                     break;
    //                 }
    //             }

    //             boardState[jumpedRow, jumpedCol] = PieceType.Empty;
    //         }
    //     }

    //     // Move the piece
    //     boardState[toRow, toCol] = boardState[fromRow, fromCol];
    //     boardState[fromRow, fromCol] = PieceType.Empty;

    //     // Promote to King
    //     if (movingPiece == PieceType.Black && toRow == 7) 
    //     {
    //         boardState[toRow, toCol] = PieceType.BlackKing;
    //         PromoteToKing(toRow, toCol, true);
    //     }
    //     else if (movingPiece == PieceType.White && toRow == 0)
    //     {
    //         boardState[toRow, toCol] = PieceType.WhiteKing;
    //         PromoteToKing(toRow, toCol, false);
    //     }

    //     Debug.Log($"Moved piece from ({fromRow}, {fromCol}) to ({toRow}, {toCol})");
    //     PrintBoard();
    // }

    private void PromoteToKing(GameObject piece)
    {
        if (kingVisualPrefab == null)
        {
            Debug.LogWarning("No kingVisualPrefab assigned!");
            return;
        }

        float height = piece.GetComponent<Collider>().bounds.size.y;

        GameObject crown = Instantiate(kingVisualPrefab, piece.transform);
        crown.transform.localPosition = new Vector3(0, height * 1.05f, 0); // place it on top
        crown.name = piece.name + "_KingVisual";
    }


    // private void PromoteToKing(int row, int col, bool isBlack)
    // {
    //     Vector3 basePos = BoardToWorld(row, col);
    //     List<GameObject> pieceList = isBlack ? blackPieces : whitePieces;

    //     foreach (var piece in pieceList)
    //     {
    //         var (r, c) = WorldToBoard(piece.transform.position);
    //         if (r == row && c == col)
    //         {
    //             // Duplicate the piece and place it above the original
    //             GameObject kingPiece = Instantiate(piece, piece.transform);
    //             float height = piece.GetComponent<Collider>().bounds.size.y;
    //             // Vector3 offsetPos = basePos + new Vector3(0, pieceHeight * 1.05f, 0); // adjust Y offset as needed
    //             kingPiece.transform.localPosition = height * 1.05f;

    //             // Optionally mark/tag the clone
    //             kingPiece.name = piece.name + "_KingMarker";
    //             kingPiece.GetComponent<Collider>().enabled = false; // prevent grabbing the crown
    //             if (kingVisual.TryGetComponent<Rigidbody>(out var rb))
    //                 rb.isKinematic = true;

    //             // Optional: set different material or color
    //             // kingPiece.GetComponent<Renderer>().material = kingMaterial;

    //             break;
    //         }
    //     }
    // }

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

    public IEnumerator MakeAIMove()
    {
        yield return new WaitForSeconds(1f); // Delay for realism

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (boardState[row, col] == PieceType.White || boardState[row, col] == PieceType.WhiteKing)
                {
                    // Try basic diagonal moves
                    for (int dRow = -1; dRow <= 1; dRow += 2)
                    {
                        for (int dCol = -1; dCol <= 1; dCol += 2)
                        {
                            int newRow = row + dRow;
                            int newCol = col + dCol;
                            if (newRow < 0 || newRow > 7 || newCol < 0 || newCol > 7)
                                continue;

                            Vector3 from = BoardToWorld(row, col);
                            Vector3 to = BoardToWorld(newRow, newCol);

                            if (IsValidMove(from, to))
                            {
                                ApplyMove(from, to);

                                // Move the white GameObject visually
                                foreach (var white in whitePieces)
                                {
                                    var (wRow, wCol) = WorldToBoard(white.transform.position);
                                    if (wRow == row && wCol == col)
                                    {
                                        white.transform.position = to;
                                        break;
                                    }
                                }

                                currentTurn = PlayerTurn.Black;
                                yield break;
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("AI could not find a valid move.");
        currentTurn = PlayerTurn.Black;
    }

}
