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
    [SerializeField] List<Row> chessBoardBoxes = null;
    [SerializeField] CHESSPIECESO[] chessPieceSO = null;
    [SerializeField] string fen = null;

    private void Start()
    {
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
        print(fen);
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
