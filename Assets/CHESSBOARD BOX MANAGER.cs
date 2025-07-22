using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CHESSBOARDBOXMANAGER : MonoBehaviour
{
    [SerializeField] CHESSBOARD chessBoard = null;
    public ChessPieceType chessPieceType;
    public ChessPieceColor pieceColor = ChessPieceColor.White;
    [SerializeField] CastlingPiecePosition castlingPiece = CastlingPiecePosition.None;
    [SerializeField] bool isCastlingPiecePosition=false;
    [SerializeField] Highlighter highlighter = null;
    [SerializeField]public PIECEMANAGER pieceInstance = null;
    [SerializeField] public bool CanActivate = false;
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
        CanActivate = state;
        //pieceInstance.ToggleDefaultLayer(state);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Piece") && CanActivate)
        {
            CanActivate = false;
            Transform targetPosition = this.transform;

            ModifiedSocket targetSocket = GetComponent<ModifiedSocket>();

            targetSocket.CanActivate = false;


                    // Check if there's a piece at the target that needs to be captured
                    PIECEMANAGER targetPiece = this.pieceInstance;
                    XRInteractionManager interactionManager = GetComponent<ModifiedSocket>().interactionManager;
                    XRGrabInteractable interactable = other.GetComponent<XRGrabInteractable>();
            // Handle capture logic if needed
                     if (targetPiece != null)
                    {
                        // Remove the target piece from the board (capture logic)
                        XRGrabInteractable targetInteractable = targetPiece.GetComponent<XRGrabInteractable>();
                        if (targetInteractable != null)
                        {

                            interactionManager.SelectExit((IXRSelectInteractor)targetSocket, (IXRSelectInteractable)targetInteractable);
                            Destroy(targetPiece.gameObject);
                        }

                         pieceInstance = null;
                    }

                    // Move the piece to the target position
                    other.transform.position = targetSocket.attachTransform != null ?
                        targetSocket.attachTransform.position : targetSocket.transform.position;

                    // Attach to the new socket using the new API with proper casting
                    interactionManager.SelectEnter((IXRSelectInteractor)targetSocket, (IXRSelectInteractable)interactable);


            // Wait for the attachment to complete
            targetSocket.CanActivate = true;
            other.GetComponent<PIECEMANAGER>().piecePlaced(this);
        }
     
            //sourceSocket.CanActivate = true;

        
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
