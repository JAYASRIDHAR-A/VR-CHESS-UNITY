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
    }
    public void NextPlayerTurn() 
    {
        if (playerToPlay == ChessPieceColor.Black) fullMove++;
        playerToPlay =playerToPlay==ChessPieceColor.White ? ChessPieceColor.Black : ChessPieceColor.White;
        FenGenerator();
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
        for(int i = 0; i < 8; i++) 
        {
            int emptyCount = 0;
            for (int j = 0; j < 8; j++)
            {
                var pieceData = chessBoardBoxes[j].columns[i].GetPieceData();
                if (pieceData.Item1 == ChessPieceType.None)
                {
                    emptyCount++;
                    continue;
                }
                else {
                    if (emptyCount != 0)
                    {
                        fen += emptyCount.ToString();
                        emptyCount = 0;
                    }
                    string symbol = GetPGNSymbol(pieceData.Item1);
                    fen += pieceData.Item2 == ChessPieceColor.White ? symbol : symbol.ToLower();

                }
            }
            if (emptyCount != 0)
            {
                fen += emptyCount.ToString();
                emptyCount = 0;
            }
            if (i < 7) 
            {
                fen += "/";
            }
        }
        fen += " " + playerToPlay.ToString()[0].ToString().ToLower() + " " + castlingRights + " " + enpassant + " " + halfMove + " " + fullMove;
        print(fen);
    }
    public void PiecePlaced() 
    {
        FenGenerator();
    }
    public void PieceGrabbed(string position) 
    {
       var pos = stockFish.GetLegalMovesForPiece(fen,position);
        print(pos);
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
