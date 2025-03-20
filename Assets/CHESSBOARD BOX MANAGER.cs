using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

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
        pieceInstance.ToggleDefaultLayer(true);
    }

    public void ToggleHighlighter(bool state) 
    {
        highlighter.enabled = state;
        if (pieceInstance == null) return;
        //pieceInstance.ToggleDefaultLayer(state);
    }
    public void PiecePlaced(SelectEnterEventArgs args) 
    {
        pieceInstance = args.interactableObject.transform.GetComponent<PIECEMANAGER>();
        pieceInstance.piecePlaced(this);
    }
    public void PieceGrabbed() 
    {
        StartCoroutine( pieceInstance.PieceGrabbed());
    }
    public void UpdatePieceInstance() 
    {
        pieceInstance = null;
    }
    public void SetupPieceManager() 
    {
        pieceInstance.SetUpPiece(castlingPiece,this);
    }
    public (ChessPieceType,ChessPieceColor) GetPieceData() 
    {
        return (chessPieceType,pieceColor);
    }

}
