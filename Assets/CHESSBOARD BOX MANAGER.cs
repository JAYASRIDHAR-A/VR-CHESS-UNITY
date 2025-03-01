using UnityEngine;

public class CHESSBOARDBOXMANAGER : MonoBehaviour
{
    [SerializeField] CHESSBOARD chessBoard = null;
    public ChessPieceType chessPieceType;
    public ChessPieceColor pieceColor = ChessPieceColor.White;

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece() 
    {
        if (chessPieceType == ChessPieceType.None) return;
        GameObject prefab = chessBoard.GetPiecePrefab((int)chessPieceType, (int)pieceColor);
        GameObject pieceInstance = Instantiate(prefab,transform.position,Quaternion.identity,transform);
    }

    public (ChessPieceType,ChessPieceColor) GetPieceData() 
    {
        return (chessPieceType,pieceColor);
    } 
}
