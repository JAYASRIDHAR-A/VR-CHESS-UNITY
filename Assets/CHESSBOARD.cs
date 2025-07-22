using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.VisualScripting;

[System.Serializable]
public class Row
{
    public List<CHESSBOARDBOXMANAGER> columns = new List<CHESSBOARDBOXMANAGER>();
}

public class CHESSBOARD : MonoBehaviour
{
   // [SerializeField] bool GameStarted = false; 
    [SerializeField] ChessPieceColor playerToPlay = ChessPieceColor.White;
    [SerializeField] StockFish stockFish = null;
    [SerializeField] public List<Vector2Int> legalPieces= null ;
    [SerializeField] List<Row> chessBoardBoxes = null;
    [SerializeField] CHESSPIECESO[] chessPieceSO = null;
    [SerializeField] string fen = null;
    [SerializeField] string castlingRights=null;
    [SerializeField] string enpassant = "-";
    [SerializeField] int halfMove = 0;
    [SerializeField] int fullMove = 1;
    [SerializeField]
    Dictionary<string, bool> movementFlags = new Dictionary<string, bool>
{
    { "WhiteKing", false }, // Tracks if the white king has moved
    { "BlackKing", false }, // Tracks if the black king has moved
    { "WhiteRookLeft", false }, // Tracks if the white rook on A1 has moved
    { "WhiteRookRight", false }, // Tracks if the white rook on H1 has moved
    { "BlackRookLeft", false }, // Tracks if the black rook on A8 has moved
    { "BlackRookRight", false }  // Tracks if the black rook on H8 has moved
};

    private void Start()
    {

        UpdateCastlingRights();
        FenGenerator();
        stockFish.SetPosition(fen);
        StartCoroutine(AIMove());
       // GetAndHighlightLegalMoves();
    }
    public void NextPlayerTurn() 
    {
        //if (playerToPlay == ChessPieceColor.Black) fullMove++;
        //playerToPlay =playerToPlay==ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
        //FenGenerator();
        GetAndHighlightLegalMoves();
    }
    public IEnumerator AIMove()
    {
        // Get the best move from Stockfish
        var task = stockFish.GetBestMoveDestination();
        yield return new WaitUntil(() => task.IsCompleted);
        var bestMove = task.Result;

        if (bestMove == null || bestMove.Count < 2)
        {
            Debug.LogError("Invalid move received from Stockfish");
            yield break;
        }

        print($"pos from: ({bestMove[0].x},{bestMove[0].y}) to: ({bestMove[1].x},{bestMove[1].y})");

        // Get the piece manager at the source position
        PIECEMANAGER sourcePiece = chessBoardBoxes[bestMove[0].x].columns[bestMove[0].y].pieceInstance;

        if (sourcePiece == null)
        {
            Debug.LogError("No piece found at source position");
            yield break;
        }

        // Get references to the source and target positions
        Transform sourcePosition = chessBoardBoxes[bestMove[0].x].columns[bestMove[0].y].transform;
        Transform targetPosition = chessBoardBoxes[bestMove[1].x].columns[bestMove[1].y].transform;

        // Get the socket interactors
        ModifiedSocket sourceSocket = sourcePosition.GetComponent<ModifiedSocket>();
        ModifiedSocket targetSocket = targetPosition.GetComponent<ModifiedSocket>();
        sourceSocket.CanActivate = false;
        targetSocket.CanActivate = false;


        if (sourceSocket != null && targetSocket != null)
        {
            // Get the interactable component from the piece
            XRGrabInteractable interactable = sourcePiece.GetComponent<XRGrabInteractable>();

            if (interactable == null)
            {
                // Try to find it in children if not on the main object
                interactable = sourcePiece.GetComponentInChildren<XRGrabInteractable>();
            }

            if (interactable != null)
            {
                // Get the interaction manager
                XRInteractionManager interactionManager = sourceSocket.interactionManager;

                // Check if there's a piece at the target that needs to be captured
                PIECEMANAGER targetPiece = chessBoardBoxes[bestMove[1].x].columns[bestMove[1].y].pieceInstance;

                // Handle capture logic if needed
                if (targetPiece != null)
                {
                    // Remove the target piece from the board (capture logic)
                    XRGrabInteractable targetInteractable = targetPiece.GetComponent<XRGrabInteractable>();
                    if (targetInteractable != null)
                    {
                        // Use the new API with proper casting
                        interactionManager.SelectExit((IXRSelectInteractor)targetSocket, (IXRSelectInteractable)targetInteractable);
                        // Handle capture (e.g., move to a "captured pieces" area or deactivate)
                        Destroy(targetPiece.gameObject);
                    }

                    // Update the board data structure
                    chessBoardBoxes[bestMove[1].x].columns[bestMove[1].y].pieceInstance = null;
                }

                // Detach from current socket using the new API with proper casting
                interactionManager.SelectExit((IXRSelectInteractor)sourceSocket, (IXRSelectInteractable)interactable);

                // Wait a frame
                yield return null;
                
                // Move the piece to the target position
                sourcePiece.transform.position = targetSocket.attachTransform != null ?
                    targetSocket.attachTransform.position : targetSocket.transform.position;

                // Attach to the new socket using the new API with proper casting
                interactionManager.SelectEnter((IXRSelectInteractor)targetSocket, (IXRSelectInteractable)interactable);

                // Update the board data structure
                //chessBoardBoxes[bestMove[1].x].columns[bestMove[1].y].pieceInstance = sourcePiece;
                //chessBoardBoxes[bestMove[0].x].columns[bestMove[0].y].pieceInstance = null;

                // Wait for the attachment to complete
                yield return null;
            }
            else
            {
                Debug.LogError("No XRGrabInteractable found on piece");
            }
           // print(sourcePiece.name);
            

        }
        sourceSocket.CanActivate = true;
        targetSocket.CanActivate = true;
       // print("Target Notation"+targetSocket.name);
        stockFish.MakeMove(sourceSocket.name+ targetSocket.name);
        NextPlayerTurn();
    }
    public void HighlightPositions(string name,bool state, List<Vector2Int> positions)
    {
       // print(name);
        //foreach (var position in positions) 
        //{
        //    print(position);
        //}
        foreach (Vector2Int move in positions)
        {
            //print(move.x+""+move.y);
            chessBoardBoxes[move.x].columns[move.y].ToggleHighlighter(state);

        }
    }
    public void HighlightPositions(bool state,List<ChessMove> postions) 
    {
        print(postions);
        foreach(ChessMove chessmove in postions) 
        {
           var move = chessmove.GetToNotation();
           chessBoardBoxes[move.x].columns[move.y].ToggleHighlighter(state);
        }
    }

    public GameObject GetPiecePrefab(int pieceIndex,int colorIndex)
    {
        if (pieceIndex < chessPieceSO.Length) 
        {
            return chessPieceSO[pieceIndex].prefab[colorIndex];
        }
        return null;
    }
    public void FenGenerator()
    {
        fen = "";
        for (int i = 7; i >= 0; i--) // Iterate over rows from top to bottom
        {
            int emptyCount = 0;
            for (int j = 0; j < 8; j++) // Iterate over columns left to right
            {
                var pieceData = chessBoardBoxes[j].columns[i].GetPieceData();
                if (pieceData.Item1 == ChessPieceType.None)
                {
                    emptyCount++; // Count empty squares
                    continue;
                }
                else
                {
                    if (emptyCount != 0)
                    {
                        fen += emptyCount.ToString(); // Add empty square count
                        emptyCount = 0;
                    }
                    string symbol = GetPGNSymbol(pieceData.Item1);
                    fen += pieceData.Item2 == ChessPieceColor.White ? symbol : symbol.ToLower(); // Append piece symbol
                }
            }
            if (emptyCount != 0)
            {
                fen += emptyCount.ToString(); // Add trailing empty square count
            }
            if (i > 0) // Add row separator if not the last row
            {
                fen += "/";
            }
        }

        // Append FEN metadata
        fen += " " + playerToPlay.ToString()[0].ToString().ToLower() + " "; // Player to play
        fen += string.IsNullOrEmpty(castlingRights) ? "-" : castlingRights; // Castling rights
        fen += " " + (string.IsNullOrEmpty(enpassant) ? "-" : enpassant); // En passant
        fen += " " + halfMove + " " + fullMove; // Halfmove and fullmove counters

        print(fen); // Print the FEN string
    }


//async void GetLegalPieces()
//{
//   await stockFish.GetLegalMoves(fen);
//}

    public void PiecePlaced() 
    {
        
    }
    public void PieceGrabbed(string position) 
    {
     // HighlightLegalPieces(true);
    }
    //public async void HighLightLegalMovesForAPiece(string position) 
    //{
    //  //  List<ChessMove> chessMoves= await stockFish.GetFullLegalMovesForPiece(fen, position);

    public List<Vector2Int> GetLegalMoves()
    {
        return legalPieces;
    }
    //}
    public void HighlightlegalPieces()
    {
        stockFish.GetPiecesWithLegalMoves().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                legalPieces = task.Result;
                // Additional code to use legalPieces here
            }
            else
            {
                Debug.LogError("Failed to get legal pieces: " + task.Exception);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    public void GetAndHighlightLegalMoves()
    {
        StartCoroutine(GetLegalMovesCoroutine());
    }

    private IEnumerator GetLegalMovesCoroutine()
    {
        // Create a task to get the legal moves
        Task<List<Vector2Int>> legalMovesTask = stockFish.GetPiecesWithLegalMoves();

        // Wait for the task to complete
        while (!legalMovesTask.IsCompleted)
        {
            yield return null;
        }

        // Check if the task completed successfully
        if (legalMovesTask.Status == TaskStatus.RanToCompletion)
        {
            // Get the result and pass it to the highlight function
            legalPieces = legalMovesTask.Result;
            HighlightPositions(this.name,true,legalPieces);
            print("LegalMoves");
        }
        else
        {
            Debug.LogError("Failed to get legal moves: " +
                (legalMovesTask.Exception != null ? legalMovesTask.Exception.Message : "Unknown error"));
        }
    }

    public void UpdateMovementFlag(string flag)
    {
        movementFlags[flag] = true;
        UpdateCastlingRights();
    }
    private void UpdateCastlingRights()
{
    string castlingRights = "";

    if (!movementFlags["WhiteKing"])
    {
        if (!movementFlags["WhiteRookRight"]) castlingRights += "K";
        if (!movementFlags["WhiteRookLeft"]) castlingRights += "Q";
    }

    if (!movementFlags["BlackKing"])
    {
        if (!movementFlags["BlackRookLeft"]) castlingRights += "k";
        if (!movementFlags["BlackRookRight"]) castlingRights += "q";
    }

    if (string.IsNullOrEmpty(castlingRights))
    {
        castlingRights = "-";
    }
        // Assign updated castling rights
        this.castlingRights = castlingRights;
    }
    private string GetPGNSymbol(ChessPieceType type)
    {
        return type switch
        {
            ChessPieceType.King => "K",
            ChessPieceType.Queen => "Q",
            ChessPieceType.Rook => "R",
            ChessPieceType.Bishop => "B",
            ChessPieceType.Knight => "N",
            ChessPieceType.Pawn => "P",
            _ => ""
        };
    }


}
