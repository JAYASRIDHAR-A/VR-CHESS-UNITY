using System;
using System.Collections.Generic;
using UnityEngine;

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

        HighlightLegalPieces();
    }
    public void NextPlayerTurn() 
    {
        if (playerToPlay == ChessPieceColor.Black) fullMove++;
        playerToPlay =playerToPlay==ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
        FenGenerator();
    }
    async void HighlightLegalPieces()
    {
        //var legalPieces = stockFish.GetPiecesWithLegalMoves(fen);
        var positions = await stockFish.GetPiecesWithLegalMoves(fen);
        print(stockFish.GetLegalMovesForPiece(fen, new Vector2Int(6,0)));
        foreach (Vector2Int move in positions)
        {
            //print(move.x+""+move.y);
            chessBoardBoxes[move.x].columns[move.y].ToggleHighlighter(true);
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
        //FenGenerator();
    }
    public void PieceGrabbed(string position) 
    {
        //var pos = stockFish.GetLegalMovesForPiece(fen, position);
        //print(pos);
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
