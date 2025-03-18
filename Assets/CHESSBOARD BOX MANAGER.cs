using UnityEngine;
using UnityEngine.UI;

public class CHESSBOARDBOXMANAGER : MonoBehaviour
{
    [SerializeField] CHESSBOARD chessBoard = null;
    public ChessPieceType chessPieceType;
    public ChessPieceColor pieceColor = ChessPieceColor.White;
    [SerializeField] CastlingPiecePosition castlingPiece = CastlingPiecePosition.None;
    [SerializeField] bool isCastlingPiecePosition=false;
    [SerializeField] Highlighter highlighter = null;
    public PIECEMANAGER pieceInstance = null;
    private void Start()
    {
        highlighter = GetComponent<Highlighter>();
        SpawnPiece();
    }

    public void SpawnPiece() 
    {
        if (chessPieceType == ChessPieceType.None) return;
        GameObject prefab = chessBoard.GetPiecePrefab((int)chessPieceType, (int)pieceColor);
        pieceInstance = Instantiate(prefab,transform.position,Quaternion.identity,transform).GetComponent<PIECEMANAGER>();
        SetupPieceManager();
    }
    public void ToggleHighlighter(bool state) 
    {
        highlighter.enabled = state;
    }
    public void PiecePlaced() 
    {
        print(this.name);
        chessBoard.PiecePlaced();
    }
    public void PieceGrabbed() 
    {
        print("Exited");
        chessBoard.PieceGrabbed(this.name);
    }
    public void SetupPieceManager() 
    {
        pieceInstance.SetUpPiece(castlingPiece,this.name);
    }
    public (ChessPieceType,ChessPieceColor) GetPieceData() 
    {
        return (chessPieceType,pieceColor);
    }

}
