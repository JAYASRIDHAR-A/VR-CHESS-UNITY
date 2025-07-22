using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class PIECEMANAGER : MonoBehaviour
{
    [SerializeField] CHESSBOARD chessBoard = null;
    [SerializeField] StockFish stockFish = null; 
    [SerializeField] UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = null;
    [SerializeField] List<Vector2Int> LegalMoves = null;
    [SerializeField] CastlingPiecePosition castlingPiece = CastlingPiecePosition.None;
    [SerializeField] CHESSBOARDBOXMANAGER prevPosition = null;
    [SerializeField] public CHESSBOARDBOXMANAGER currentPosition = null;
    [SerializeField] int totalSteps = 0;
    [SerializeField] bool isTriggered = false;

    private void Start()
    {
        chessBoard = FindAnyObjectByType<CHESSBOARD>();
        stockFish = FindAnyObjectByType<StockFish>();
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
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
        print("Grabbed");
        var task = stockFish.GetFullLegalMovesForPiece(currentPosition.gameObject.name);
     // print("test" + stockFish.GetFullLegalMovesForPiece(currentPosition.gameObject.name));
        yield return new WaitUntil(() => task.IsCompleted);
        LegalMoves = task.Result;
        chessBoard.HighlightPositions(this.name,false, chessBoard.GetLegalMoves());
        print(LegalMoves);
        chessBoard.HighlightPositions(this.name, true, LegalMoves);
        // rest of your code
    }
    public void piecePlaced(CHESSBOARDBOXMANAGER position) 
    {
        print("placed");
        UpdatePieceData(position);
        print(LegalMoves);
        chessBoard.HighlightPositions(this.name,false, LegalMoves);
        stockFish.MakeMove(prevPosition.gameObject.name+currentPosition.gameObject.name);
       StartCoroutine( chessBoard. AIMove());
       // chessBoard.NextPlayerTurn();

    }
    public void ToggleDefaultLayer(bool includeDefault)
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        int defaultLayerValue = 1 << LayerMask.NameToLayer("Default");

        if (includeDefault)
            interactable.interactionLayers |= defaultLayerValue;  // Add Default
        else
            interactable.interactionLayers &= ~defaultLayerValue;  // Remove Default
    }
}
