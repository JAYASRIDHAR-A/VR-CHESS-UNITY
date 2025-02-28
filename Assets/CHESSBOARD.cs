using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Row
{
    public List<CHESSBOARDBOXMANAGER> columns = new List<CHESSBOARDBOXMANAGER>();
}

public class CHESSBOARD : MonoBehaviour
{
    [SerializeField]
    private List<Row> chessBoardBoxmanagers = new List<Row>();

    public List<Row> ChessBoardBoxManagers => chessBoardBoxmanagers;

    [SerializeField]
    private CHESSPIECESO[] chessPieceSO = new CHESSPIECESO[8]; // Array initialized

    public CHESSPIECESO[] ChessPieceSO => chessPieceSO;

    [SerializeField] StockFish StockFish = null;

    //public ChessPieceType GetPieceData(ChessPieceType type)
    //{
    //    return chessPieceDataArray[(int)type];
    //}

    public void Reset()
    {
        // Initialize an 8x8 grid
        chessBoardBoxmanagers.Clear();
        for (int i = 0; i < 8; i++)
        {
            var row = new Row();
            for (int j = 0; j < 8; j++)
            {
                row.columns.Add(null);
            }
            chessBoardBoxmanagers.Add(row);
        }
    }
    private void Start()
    {
        GenerateFEN();
    }
    public string GenerateFEN()
    {
        string fen = "";

        for (int row = 0; row < 8; row++)
        {
            int emptyCount = 0;
            for (int col = 0; col < 8; col++)
            {
                var boxManager = chessBoardBoxmanagers[row].columns[col];
                var piece = boxManager?.chessPieceType ?? ChessPieceType.None;

                if (piece == ChessPieceType.None)
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount > 0)
                    {
                        fen += emptyCount.ToString();
                        emptyCount = 0;
                    }
                    // Add piece symbol
                    string symbol = ChessPiecePGNMapper.GetPGNSymbol(piece);

                    // Lowercase for black pieces, uppercase for white
                    fen += (boxManager != null && boxManager.IsWhite) ? symbol.ToUpper() : symbol.ToLower();
                }
            }

            if (emptyCount > 0)
            {
                fen += emptyCount.ToString();
            }

            if (row < 7)
            {
                fen += "/";
            }
        }

        fen += " w KQkq - 0 1";
        print(fen);
        string bestmove = StockFish.GetBestMove(fen);
        print(bestmove);
        return fen;
    }
    public GameObject GetPrefab(ChessPieceType type, bool pieceColor)
    {
        if ((int)type < chessPieceSO.Length)
        {
            return chessPieceSO[(int)type].prefab[pieceColor ? 0 : 1];
        }
        return null;
    }
}
