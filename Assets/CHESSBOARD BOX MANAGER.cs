using UnityEngine;

public class CHESSBOARDBOXMANAGER : MonoBehaviour
{
    [SerializeField] CHESSBOARD chessBoard = null;
    public ChessPieceType chessPieceType;
    public bool IsWhite = true;

    private void Start()
    {
        if (chessPieceType != ChessPieceType.None)
        {
           GameObject piece =  chessBoard.GetPrefab(chessPieceType, IsWhite);
            //Vector3 adjustedPosition = transform.position + new Vector3(0,0, 0.285f);
            GameObject instance = Instantiate(piece,transform.position,piece.transform.rotation,transform);
           // instance.transform.localPosition = adjustedPosition;

        }
    }
}
