using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class StockFish : MonoBehaviour
{
    private Process stockfishProcess;
    private StreamWriter stockfishInput;
    private StreamReader stockfishOutput;

    // Current position state
    private string currentFen;
    private List<string> moveHistory = new List<string>();

    private void Awake()
    {
        StartStockfish();
    }

    private void OnApplicationQuit()
    {
        CloseStockfish();
    }

    public void StartStockfish()
    {
        string fileName = @"D:\AJ\GIT\VR-CHESS-UNITY\Assets\Stockfish\stockfish.exe";

        try
        {
            stockfishProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            stockfishProcess.Start();
            stockfishInput = stockfishProcess.StandardInput;
            stockfishOutput = stockfishProcess.StandardOutput;

            UnityEngine.Debug.Log("Stockfish started successfully.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to start Stockfish: {e.Message}");
        }
    }

    public void CloseStockfish()
    {
        try
        {
            stockfishInput?.Close();
            stockfishOutput?.Close();
            stockfishProcess?.Kill();
            stockfishProcess?.Dispose();
            UnityEngine.Debug.Log("Stockfish closed successfully.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error while closing Stockfish: {e.Message}");
        }
    }

    // Initialize the position with a FEN string
    public void SetPosition(string fen)
    {
        currentFen = fen;
        moveHistory.Clear();
        SendCommand($"position fen {fen}");
        UnityEngine.Debug.Log($"Initial position set: {fen}");
    }

    // Make a move and update the internal position
    public void MakeMove(string move)
    {
        if (string.IsNullOrEmpty(currentFen))
        {
            UnityEngine.Debug.LogError("Position not initialized. Call SetPosition first.");
            return;
        }

        moveHistory.Add(move);
        SendCommand($"position fen {currentFen} moves {string.Join(" ", moveHistory)}");
        UnityEngine.Debug.Log($"Move made: {move}. Total moves: {moveHistory.Count}");
    }

    // Reset the position to the initial FEN
    public void ResetPosition()
    {
        if (!string.IsNullOrEmpty(currentFen))
        {
            moveHistory.Clear();
            SendCommand($"position fen {currentFen}");
            UnityEngine.Debug.Log("Position reset to initial state");
        }
    }

    // Undo the last move
    public void UndoMove()
    {
        if (moveHistory.Count > 0)
        {
            moveHistory.RemoveAt(moveHistory.Count - 1);
            if (moveHistory.Count > 0)
            {
                SendCommand($"position fen {currentFen} moves {string.Join(" ", moveHistory)}");
            }
            else
            {
                SendCommand($"position fen {currentFen}");
            }
            UnityEngine.Debug.Log("Last move undone");
        }
    }

    // Get the current FEN string from Stockfish
    public async Task<string> GetCurrentFen()
    {
        SendCommand("d");
        string response = await WaitForResponse();

        string fen = null;
        string[] lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            if (line.StartsWith("Fen:"))
            {
                fen = line.Substring(5).Trim();
                break;
            }
        }

        return fen;
    }

    public async Task<List<UnityEngine.Vector2Int>> GetPiecesWithLegalMoves()
    {
        SendCommand("go perft 1");

        string response = await WaitForResponse();
        if (string.IsNullOrEmpty(response))
        {
            UnityEngine.Debug.LogWarning("No response received from Stockfish.");
            return new List<UnityEngine.Vector2Int>();  // Return empty List
        }

        var positions = ParsePerftOutput(response);
        UnityEngine.Debug.Log($"Pieces with legal moves: {string.Join(", ", positions)}");

        // Convert HashSet to List before returning
        return new List<UnityEngine.Vector2Int>(positions);
    }

    // Get legal moves for a specific piece as destination squares
    public async Task<string[]> GetLegalMovesForPiece(string squareNotation)
    {
        SendCommand("go perft 1");

        string response = await WaitForResponse();
        if (string.IsNullOrEmpty(response))
        {
            UnityEngine.Debug.LogWarning("No response received from Stockfish.");
            return new string[0];
        }

        // Parse only moves that start with the specified position
        List<string> destinationSquares = new List<string>();
        string[] lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            if (line.Contains(":"))
            {
                string moveText = line.Split(':')[0].Trim(); // e.g., "a2a3"

                // Check if this move starts with our target square
                if (moveText.StartsWith(squareNotation) && moveText.Length >= 4)
                {
                    // Extract destination square
                    string toSquare = moveText.Substring(2, 2);   // e.g., "a3"
                    destinationSquares.Add(toSquare);
                }
            }
        }

        UnityEngine.Debug.Log($"Legal moves for piece at {squareNotation}: {string.Join(", ", destinationSquares)}");
        return destinationSquares.ToArray();
    }

    // Get full move information for a specific piece
    public async Task<List<ChessMove>> GetFullLegalMovesForPiece(string squareNotation)
    {
        SendCommand("go perft 1");

        string response = await WaitForResponse();
        if (string.IsNullOrEmpty(response))
        {
            UnityEngine.Debug.LogWarning("No response received from Stockfish.");
            return new List<ChessMove>();
        }

        // Parse only moves that start with the specified position
        List<ChessMove> legalMoves = ParseMovesForSquare(response, squareNotation);
        UnityEngine.Debug.Log($"Legal moves for piece at {squareNotation}: {string.Join(", ", legalMoves)}");

        return legalMoves;
    }

    // Check if a king is in check
    public async Task<bool> IsInCheck()
    {
        SendCommand("d"); // "d" command displays the current position info

        string response = await WaitForResponse();
        if (string.IsNullOrEmpty(response))
        {
            UnityEngine.Debug.LogWarning("No response received from Stockfish.");
            return false;
        }

        // Parse the response to find if the king is in check
        return response.Contains("Checkers:") && !response.Contains("Checkers: "); // The line "Checkers: " (empty) means no check
    }

    // Get the best move from Stockfish
    public async Task<ChessMove> GetBestMove(int depth = 12)
    {
        SendCommand($"go depth {depth}");

        string response = await WaitForResponse();
        if (string.IsNullOrEmpty(response))
        {
            UnityEngine.Debug.LogWarning("No response received from Stockfish.");
            return null;
        }

        // Parse to find the best move line
        string bestMove = null;
        string[] lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            if (line.StartsWith("bestmove"))
            {
                bestMove = line.Split(' ')[1]; // e.g., "bestmove e2e4"
                break;
            }
        }

        if (string.IsNullOrEmpty(bestMove) || bestMove.Length < 4)
        {
            UnityEngine.Debug.LogWarning("No best move found in Stockfish response.");
            return null;
        }

        // Parse the move into a ChessMove object
        string fromSquare = bestMove.Substring(0, 2);
        string toSquare = bestMove.Substring(2, 2);

        ChessMove move = new ChessMove
        {
            From = ConvertTo2DPosition(fromSquare),
            To = ConvertTo2DPosition(toSquare),
            FromNotation = fromSquare,
            ToNotation = toSquare
        };

        // Handle promotion if present
        if (bestMove.Length > 4)
        {
            move.Promotion = bestMove[4];
        }

        // Get current FEN to check for special moves
        string currentFenString = await GetCurrentFen();

        // Check for special moves
        if (IsCastling(fromSquare, toSquare))
        {
            move.IsCastling = true;
        }

        if (IsEnPassant(currentFenString, fromSquare, toSquare))
        {
            move.IsEnPassant = true;
        }

        return move;
    }

    // Parse only moves that start with the specified square
    private List<ChessMove> ParseMovesForSquare(string response, string startSquare)
    {
        List<ChessMove> moves = new List<ChessMove>();
        string[] lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            if (line.Contains(":"))
            {
                string moveText = line.Split(':')[0].Trim(); // e.g., "a2a3"

                // Check if this move starts with our target square
                if (moveText.StartsWith(startSquare) && moveText.Length >= 4)
                {
                    // Parse the move
                    string fromSquare = moveText.Substring(0, 2); // e.g., "a2"
                    string toSquare = moveText.Substring(2, 2);   // e.g., "a3"

                    // Check if it's a promotion move
                    char? promotion = null;
                    if (moveText.Length > 4)
                    {
                        promotion = moveText[4];
                    }

                    // Create a ChessMove object
                    ChessMove move = new ChessMove
                    {
                        From = ConvertTo2DPosition(fromSquare),
                        To = ConvertTo2DPosition(toSquare),
                        FromNotation = fromSquare,
                        ToNotation = toSquare,
                        Promotion = promotion
                    };

                    // Check for special moves
                    if (IsCastling(fromSquare, toSquare))
                    {
                        move.IsCastling = true;
                    }

                    // Check for en passant
                    if (IsPawnMove(fromSquare, toSquare) &&
                        Math.Abs(fromSquare[0] - toSquare[0]) == 1 &&
                        Math.Abs(fromSquare[1] - toSquare[1]) == 1)
                    {
                        move.IsEnPassant = true;
                    }

                    moves.Add(move);
                }
            }
        }
        print(moves);
        return moves;
    }

    // Check if a move is a pawn move
    private bool IsPawnMove(string from, string to)
    {
        // Simple check - if the move is vertical or diagonal by one square
        // This is just a heuristic, not a perfect check
        return Math.Abs(from[0] - to[0]) <= 1 &&
               Math.Abs(from[1] - to[1]) <= 2;
    }

    // Check if a move is an en passant capture
    private bool IsEnPassant(string fen, string from, string to)
    {
        // Parse the FEN to get the en passant target square
        string[] fenParts = fen.Split(' ');
        if (fenParts.Length < 6)
            return false;

        string enPassantSquare = fenParts[3];

        // If there's no en passant target, it can't be an en passant move
        if (enPassantSquare == "-")
            return false;

        // Check if the move is a pawn move to the en passant target
        char fromFile = from[0];
        char toFile = to[0];

        return to == enPassantSquare &&
               Math.Abs(fromFile - toFile) == 1 &&
               (from[1] == '4' || from[1] == '5'); // Only pawns on the 4th or 5th rank can perform en passant
    }

    // Simple check for castling (can be expanded for more accurate detection)
    private bool IsCastling(string from, string to)
    {
        // King moving two squares horizontally is likely castling
        if (from[0] == 'e' && (to[0] == 'g' || to[0] == 'c'))
        {
            // White king initial position
            if (from == "e1" && (to == "g1" || to == "c1"))
                return true;
            // Black king initial position
            if (from == "e8" && (to == "g8" || to == "c8"))
                return true;
        }
        return false;
    }

    private void SendCommand(string command)
    {
        try
        {
            stockfishInput.WriteLine(command);
            stockfishInput.Flush();
            UnityEngine.Debug.Log($"Command sent to Stockfish: {command}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to send command to Stockfish: {e.Message}");
        }
    }

    private async Task<string> WaitForResponse()
    {
        string output = string.Empty;
        string line;
        //bool readingPerft = false;
       // int nodeCount = 0;

        UnityEngine.Debug.Log("Waiting for response from Stockfish...");
        while ((line = await stockfishOutput.ReadLineAsync()) != null)
        {
            UnityEngine.Debug.Log($"Stockfish Output: {line}"); // Log each line from Stockfish
            output += line + "\n";

            // Handle different commands
            if (line.StartsWith("Nodes searched"))
            {
                // End of perft output
                break;
            }
            else if (line.StartsWith("bestmove"))
            {
                // End of go command output
                break;
            }
            else if (line.StartsWith("Total: "))
            {
                // End position display output
                break;
            }
            else if (line.Contains("Checkers:"))
            {
                // End of position display output for check detection
                break;
            }
        }

        if (string.IsNullOrEmpty(output))
        {
            UnityEngine.Debug.LogWarning("Stockfish response is empty.");
        }

        return output;
    }

    private HashSet<UnityEngine.Vector2Int> ParsePerftOutput(string response)
    {
        HashSet<UnityEngine.Vector2Int> positions = new HashSet<UnityEngine.Vector2Int>();
        string[] lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            if (line.Contains(":"))
            {
                string move = line.Split(':')[0].Trim(); // e.g., "a2a3"

                // Only process valid chess squares (length should be at least 2 characters)
                if (move.Length >= 2 && move[0] >= 'a' && move[0] <= 'h' && move[1] >= '1' && move[1] <= '8')
                {
                    // Convert to 2D position
                    UnityEngine.Vector2Int position = ConvertTo2DPosition(move.Substring(0, 2)); // e.g., "a2"
                    positions.Add(position);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Invalid move detected: {move}");
                }
            }
        }

        return positions;
    }

    private bool IsValidSquare(string square)
    {
        // Validate chess square (e.g., "a1" to "h8")
        if (square.Length != 2) return false;

        char file = square[0]; // 'a' to 'h'
        char rank = square[1]; // '1' to '8'

        return file >= 'a' && file <= 'h' && rank >= '1' && rank <= '8';
    }

    private UnityEngine.Vector2Int ConvertTo2DPosition(string square)
    {
        if (square.Length != 2 || square[0] < 'a' || square[0] > 'h' || square[1] < '1' || square[1] > '8')
        {
            throw new ArgumentException($"Invalid chess square: {square}");
        }

        char file = square[0];
        char rank = square[1];

        int i = (rank - '0') - 1; // Convert rank to row (0-indexed, reversed)
        int j = file - 'a';     // Convert file to column (0-indexed)

        return new UnityEngine.Vector2Int(j, i); // Return as Vector2Int (x, y)
    }

    // Convert Vector2Int to chess notation
    private string ConvertToSquareNotation(Vector2Int position)
    {
        char file = (char)('a' + position.x);
        char rank = (char)('1' + position.y);
        return $"{file}{rank}";
    }

    // Helper method to extract just the destination squares from a list of ChessMoves
    public string[] GetDestinationSquares(List<ChessMove> moves)
    {
        return moves.Select(move => move.ToNotation).ToArray();
    }

    // Evaluate the current position (positive for white advantage, negative for black)
    public async Task<float> EvaluatePosition(int depth = 16)
    {
        SendCommand($"go depth {depth}");

        string response = await WaitForResponse();
        if (string.IsNullOrEmpty(response))
        {
            UnityEngine.Debug.LogWarning("No response received from Stockfish.");
            return 0.0f;
        }

        // Parse the evaluation
        float evaluation = 0.0f;
        string[] lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            if (line.Contains("score cp"))
            {
                // Extract the centipawn score
                string[] parts = line.Split(' ');
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (parts[i] == "cp")
                    {
                        if (float.TryParse(parts[i + 1], out float score))
                        {
                            evaluation = score / 100.0f; // Convert from centipawns to pawns
                            break;
                        }
                    }
                }
            }
            else if (line.Contains("score mate"))
            {
                // Extract the mate score
                string[] parts = line.Split(' ');
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (parts[i] == "mate")
                    {
                        if (int.TryParse(parts[i + 1], out int moves))
                        {
                            // Use a large value for mate, with sign indicating which side is winning
                            evaluation = moves > 0 ? 100.0f : -100.0f;
                            break;
                        }
                    }
                }
            }
        }

        return evaluation;
    }
}

// Represents a chess move with full information
[System.Serializable]
public class ChessMove
{
    public Vector2Int From;
    public Vector2Int To;
    public string FromNotation;
    public string ToNotation;
    public char? Promotion;
    public bool IsCastling;
    public bool IsEnPassant;

    public override string ToString()
    {
        string moveStr = $"{FromNotation}{ToNotation}";
        if (Promotion.HasValue)
            moveStr += Promotion;
        return moveStr;
    }

    // Helper method to get the move in long algebraic notation
    public string GetLongAlgebraicNotation()
    {
        return $"{FromNotation}{ToNotation}{(Promotion.HasValue ? Promotion.ToString() : "")}";
    }

    // Helper method to get the displacement vector
    public Vector2Int GetDisplacement()
    {
        return new Vector2Int(To.x - From.x, To.y - From.y);
    }
    public UnityEngine.Vector2Int GetToNotation()
    {
        if (ToNotation.Length != 2 || ToNotation[0] < 'a' || ToNotation[0] > 'h' || ToNotation[1] < '1' || ToNotation[1] > '8')
        {
            throw new ArgumentException($"Invalid chess square: {ToNotation}");
        }

        char file = ToNotation[0];
        char rank = ToNotation[1];

        int i = (rank - '0') - 1; // Convert rank to row (0-indexed, reversed)
        int j = file - 'a';     // Convert file to column (0-indexed)

        return new UnityEngine.Vector2Int(j, i); // Return as Vector2Int (x, y)
    }
}