using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PIECEMANAGER : MonoBehaviour
{
    [SerializeField] CHESSBOARD chessBoard = null;
    [SerializeField] StockFish stockFish = null; 
    [SerializeField] XRGrabInteractable interactable = null;
    [SerializeField] List<ChessMove> LegalMoves = null;
    [SerializeField] CastlingPiecePosition castlingPiece = CastlingPiecePosition.None;
    [SerializeField] CHESSBOARDBOXMANAGER prevPosition = null;
    [SerializeField] CHESSBOARDBOXMANAGER currentPosition = null;
    [SerializeField] int totalSteps = 0;
    [SerializeField] bool isTriggered = false;


    private void Start()
    {
        chessBoard = FindAnyObjectByType<CHESSBOARD>();
        stockFish = FindAnyObjectByType<StockFish>();
        interactable = GetComponent<XRGrabInteractable>();
    }
    public void SetUpPiece(CastlingPiecePosition castlingPieceEnum,CHESSBOARDBOXMANAGER position)
    {
        currentPosition = position;
        castlingPiece = castlingPieceEnum;
    }

    public void UpdatePieceData(CHESSBOARDBOXMANAGER position)
    {
        currentPosition.UpdatePieceInstance();
        prevPosition = currentPosition;
        currentPosition = position;
        totalSteps++;
    }
    //public bool PawnDisplacemant() 
    //{
    //    return (int)prevPosition[1]+2==(int)currentPosition[1];
    //}
    public IEnumerator PieceGrabbed()
    {
        var task = stockFish.GetFullLegalMovesForPiece(currentPosition.gameObject.name);
        print("test" + stockFish.GetLegalMovesForPiece(currentPosition.gameObject.name));
        yield return new WaitUntil(() => task.IsCompleted);
        LegalMoves = task.Result;
        chessBoard.HighlightPositions(false, chessBoard.GetLegalMoves());
        chessBoard.HighlightPositions(true, LegalMoves);
        // rest of your code
    }
    public void piecePlaced(CHESSBOARDBOXMANAGER position) 
    {
        UpdatePieceData(position);
        chessBoard.HighlightPositions(false, LegalMoves);
        stockFish.MakeMove(prevPosition.gameObject.name+currentPosition.gameObject.name);
        chessBoard.NextPlayerTurn();
    }
    public void ToggleDefaultLayer(bool includeDefault)
    {
        var interactable = GetComponent<XRGrabInteractable>();
        int defaultLayerValue = 1 << LayerMask.NameToLayer("Default");

        if (includeDefault)
            interactable.interactionLayers |= defaultLayerValue;  // Add Default
        else
            interactable.interactionLayers &= ~defaultLayerValue;  // Remove Default
    }
}
