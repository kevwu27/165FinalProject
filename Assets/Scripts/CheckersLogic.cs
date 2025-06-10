using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Utilities;
using Meta.WitAi.TTS.Data;

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

    public GameObject board;

    public PlayerTurn currentTurn = PlayerTurn.Black;
    private Dictionary<GameObject, (int row, int col)> piecePositions = new();

    public GameObject blackKingPrefab;
    public GameObject whiteKingPrefab;
    public GameObject blackCheckerPrefab;
    public GameObject whiteCheckerPrefab;
    public AudioSource kingSound;

    public RoundEndMenu roundEndMenu;
    public bool gameOver = false;

    private List<GameObject> blackPieces = new();
    private List<GameObject> whitePieces = new();

    public Animator agentAnimator;
    public TTSSpeaker speaker;

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
        blackPieces.Clear();
        whitePieces.Clear();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 1) // valid checker cell
                {
                    Vector3 worldPos = BoardToWorld(row, col);

                    if (row <= 2)
                    {
                        GameObject blackPiece = Instantiate(blackCheckerPrefab, worldPos, Quaternion.Euler(-90, 0, 0));
                        blackPiece.tag = "CheckersPieceBlack";
                        blackPieces.Add(blackPiece);
                        boardState[row, col] = PieceType.Black;
                        piecePositions[blackPiece] = (row, col);
                    }
                    else if (row >= 5)
                    {
                        GameObject whitePiece = Instantiate(whiteCheckerPrefab, worldPos, Quaternion.Euler(-90, 0, 0));
                        whitePiece.tag = "CheckersPieceWhite";
                        whitePieces.Add(whitePiece);
                        boardState[row, col] = PieceType.White;
                        piecePositions[whitePiece] = (row, col);
                    }
                    else
                    {
                        boardState[row, col] = PieceType.Empty;
                    }
                }
                else
                {
                    boardState[row, col] = PieceType.Empty;
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
                if (isKing || rowDelta == dir * 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void ApplyMove(Vector3 fromWorldPos, Vector3 toWorldPos)
    {
        if (gameOver) return;
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

                    agentAnimator.SetBool("ConversationMode", false);
                    if (currentTurn == PlayerTurn.Black)
                    {
                        speaker.Speak("Aww darn, you got me!");
                        // Player captured AI piece
                        agentAnimator.SetBool("PlayerGood", true);
                        StartCoroutine(ResetEmotion("PlayerGood", 5f));
                    }
                    else
                    {   
                        speaker.Speak("Another one bites the dust.");
                        // AI captured player piece
                        agentAnimator.SetBool("AIGood", true);
                        StartCoroutine(ResetEmotion("AIGood", 5f));
                    }
                    

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

        if (blackPieces.Count == 0 || whitePieces.Count == 0)
        {
            gameOver = true;
            roundEndMenu.menuCanvas.SetActive(true);
            bool playerWon = (whitePieces.Count == 0);
            roundEndMenu.ShowEndScreen(playerWon);
            Debug.Log($"menuCanvas active? {roundEndMenu.menuCanvas.activeSelf}, menuAnimator exists? {roundEndMenu.menuAnimator != null}");
        }
    }

    IEnumerator ResetEmotion(string paramName, float delay)
    {
        yield return new WaitForSeconds(delay);
        agentAnimator.SetBool(paramName, false);
        agentAnimator.SetBool("ConversationMode", true);
    }


    private void PromoteToKing(GameObject originalPiece)
    {
        if (!piecePositions.TryGetValue(originalPiece, out var pos))
        {
            Debug.LogWarning("Could not find piece in position map.");
            return;
        }

        int row = pos.row;
        int col = pos.col;

        GameObject kingPrefab = (boardState[row, col] == PieceType.BlackKing) ? blackKingPrefab : whiteKingPrefab;
        if (kingPrefab == null)
        {
            Debug.LogWarning("Missing king prefab reference!");
            return;
        }

        // Spawn king in same position
        Vector3 kingPos = BoardToWorld(row, col);
        Quaternion kingRot = originalPiece.transform.rotation;
        GameObject kingPiece = Instantiate(kingPrefab, kingPos, kingRot);

        // Replace in the right list
        List<GameObject> pieceList = boardState[row, col] > 0 ? blackPieces : whitePieces;
        pieceList.Remove(originalPiece);
        pieceList.Add(kingPiece);

        // Update position tracking
        piecePositions.Remove(originalPiece);
        piecePositions[kingPiece] = (row, col);

        Destroy(originalPiece);
        kingSound?.Play();
    }

    public string PrintBoard()
    {
        string[,] symbols = new string[8,8];

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                switch ((int)boardState[row, col])
                {
                    case 1:  symbols[row, col] = "b"; break; // Black
                    case 2:  symbols[row, col] = "B"; break; // Black King
                    case -1: symbols[row, col] = "w"; break; // White
                    case -2: symbols[row, col] = "W"; break; // White King
                    default: symbols[row, col] = "."; break; // Empty
                }
            }
        }

        string print = "    A  B  C  D  E  F  G  H\n";
        print += "  +------------------------+\n";

        for (int row = 7; row >= 0; row--)
        {
            print += (row + 1) + " | ";
            for (int col = 0; col < 8; col++)
            {
                print += symbols[row, col] + "  ";
            }
            print += "|\n";
            
        }
        Debug.Log(print);

        string result = "  +------------------------+\n\n";
        result += "Legend:\n";
        result += "b = Black, B = Black King\n";
        result += "w = White, W = White King\n";
        result += ". = Empty\n";

        result += "\nPiece Positions:\n";
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int piece = (int)boardState[row, col];
                if (piece != 0)
                {
                    char colLetter = (char)('A' + col);
                    int rowNumber = row + 1;
                    string pieceSymbol = piece switch
                    {
                        1 => "b",  // Black
                        2 => "B",  // Black King
                        -1 => "w", // White
                        -2 => "W", // White King
                        _ => "."
                    };
                    result += $"{pieceSymbol} at {colLetter}{rowNumber}\n";
                }
            }
        }
        Debug.Log(result);

        return result;
    }

    public IEnumerator MakeAIMove()
    {
        yield return new WaitForSeconds(1f); // Delay for realism

        List<(Vector3 from, Vector3 to)> capturingMoves = new();
        List<(Vector3 from, Vector3 to)> normalMoves = new();

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                PieceType piece = boardState[row, col];

                if (piece == PieceType.White || piece == PieceType.WhiteKing)
                {
                    Vector3 from = BoardToWorld(row, col);

                    int[] dRows = piece == PieceType.White ? new int[] { -1, -2 } : new int[] { -1, 1, -2, 2 };
                    int[] dCols = new int[] { -1, 1, -2, 2 };

                    foreach (int dRow in dRows)
                    {
                        foreach (int dCol in dCols)
                        {
                            int newRow = row + dRow;
                            int newCol = col + dCol;

                            if (newRow < 0 || newRow > 7 || newCol < 0 || newCol > 7)
                            {
                                continue;
                            }

                            Vector3 to = BoardToWorld(newRow, newCol);

                            if (IsValidMove(from, to))
                            {
                                if (Mathf.Abs(newRow - row) == 2) // capturing move
                                {
                                    capturingMoves.Add((from, to));
                                }
                                else
                                {
                                    normalMoves.Add((from, to));
                                }
                            }
                        }
                    }
                }
            }
        }

        List<(Vector3 from, Vector3 to)> movePool = capturingMoves.Count > 0 ? capturingMoves : normalMoves;

        if (movePool.Count > 0)
        {
            var chosen = movePool[Random.Range(0, movePool.Count)];
            ApplyMove(chosen.from, chosen.to);
        }
        else
        {
            Debug.Log("AI could not find a valid move.");
        }
        currentTurn = PlayerTurn.Black;
    }

    public void RestartGame()
    {
        gameOver = false;
        // 1. Destroy all checker pieces
        foreach (GameObject piece in blackPieces)
            Destroy(piece);
        foreach (GameObject piece in whitePieces)
            Destroy(piece);

        // 2. Clear lists and position map
        blackPieces.Clear();
        whitePieces.Clear();
        piecePositions.Clear();

        // 3. Clear board state
        for (int row = 0; row < 8; row++)
            for (int col = 0; col < 8; col++)
                boardState[row, col] = PieceType.Empty;

        // 4. Respawn fresh pieces
        SpawnPieces();

        // 5. Reset turn
        currentTurn = PlayerTurn.Black;
        // Reset round end menu animation state
        roundEndMenu.menuAnimator.Rebind();  // Resets all parameters and states

        Debug.Log("Game restarted.");

    }
}
