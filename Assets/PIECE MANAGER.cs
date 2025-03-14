using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIECEMANAGER : MonoBehaviour
{
    [SerializeField] CastlingPiecePosition castlingPiece = CastlingPiecePosition.None;
    [SerializeField] string prevPosition=null;
    [SerializeField] string currentPosition=null;
    [SerializeField] int  totalSteps=0;
    [SerializeField] bool isTriggered = false;
    [SerializeField] CHESSBOARD chessBoard = null;

    private void Start()
    {
        chessBoard = GameObject.FindWithTag("ChessBoard").GetComponent<CHESSBOARD>();
    }
    public void SetUpPiece(CastlingPiecePosition castlingPieceEnum, string position) 
    {
        currentPosition = position;
        castlingPiece = castlingPieceEnum;
    }

    public void UpdatePieceData(string position) 
    {
        prevPosition = currentPosition;
        currentPosition = position;
        totalSteps++;
    }
    public bool PawnDisplacemant() 
    {
        return (int)prevPosition[1]+2==(int)currentPosition[1];
    }


}
