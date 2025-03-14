using UnityEngine;

[CreateAssetMenu(fileName = "NewChessPiece", menuName = "Chess/Chess Piece")]
public class CHESSPIECESO : ScriptableObject
{
    [Header("Piece Info")]
    public ChessPieceType pieceType; // Enum for the type of piece
    public string pieceName;
    [TextArea]
    public string description;

    [Header("Piece Prefabs")]
    public GameObject[] prefab= new GameObject[2];

}
public enum ChessPieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King,
    None
}
public enum ChessPieceColor 
{
    Black,
    White
}
public enum CastlingPiecePosition
{
    None,
    WhiteKing,
    BlackKing,
    WhiteRookLeft,
    WhiteRookRight,
    BlackRookLeft,
    BlackRookRight
}

