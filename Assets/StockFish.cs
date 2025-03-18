using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class StockFish : MonoBehaviour
{
    private Process stockfishProcess;
    private StreamWriter stockfishInput;
    private StreamReader stockfishOutput;

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
        string fileName = @"D:\GIT\VR-CHESS-UNITY\Assets\Stockfish\stockfish.exe";

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

    public async Task<List<UnityEngine.Vector2Int>> GetPiecesWithLegalMoves(string fen)
    {
        SendCommand($"position fen {fen}");
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

    // NEW METHOD: Get legal moves for a specific piece
    public async Task<List<Move>> GetLegalMovesForPiece(string fen, Vector2Int piecePosition)
    {
        // Convert Vector2Int position to chess notation (e.g., (0,0) -> "a1")
        string squareNotation = ConvertToSquareNotation(piecePosition);

        // Get all legal moves in the position
        SendCommand($"position fen {fen}");
        SendCommand("go perft 1");

        string response = await WaitForResponse();
        if (string.IsNullOrEmpty(response))
        {
            UnityEngine.Debug.LogWarning("No response received from Stockfish.");
            return new List<Move>();
        }

        // Parse only moves that start with the specified position
        List<Move> legalMoves = ParseMovesForSquare(response, squareNotation);
UnityEngine.Debug.Log($"Legal moves for piece at {squareNotation}: {string.Join(", ", legalMoves)}");

        return legalMoves;
    }

    // Parse only moves that start with the specified square
    private List<Move> ParseMovesForSquare(string response, string startSquare)
    {
        List<Move> moves = new List<Move>();
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

                    // Create a Move object
                    Move move = new Move
                    {
                        From = ConvertTo2DPosition(fromSquare),
                        To = ConvertTo2DPosition(toSquare),
                        FromNotation = fromSquare,
                        ToNotation = toSquare,
                        Promotion = promotion
                    };

                    moves.Add(move);
                }
            }
        }

        return moves;
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

        UnityEngine.Debug.Log("Waiting for response from Stockfish...");
        while ((line = await stockfishOutput.ReadLineAsync()) != null)
        {
            UnityEngine.Debug.Log($"Stockfish Output: {line}"); // Log each line from Stockfish
            output += line + "\n";
            if (line.StartsWith("Nodes searched")) // End of perft output
                break;
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
        int j = file - 'a';       // Convert file to column (0-indexed)

        return new UnityEngine.Vector2Int(j, i); // Return as Vector2Int (x, y)
    }

    // NEW METHOD: Convert Vector2Int to chess notation
    private string ConvertToSquareNotation(Vector2Int position)
    {
        char file = (char)('a' + position.x);
        char rank = (char)('1' + position.y);
        return $"{file}{rank}";
    }
}

// NEW CLASS: Represents a chess move
[System.Serializable]
public class Move
{
    public Vector2Int From;
    public Vector2Int To;
    public string FromNotation;
    public string ToNotation;
    public char? Promotion;

    public override string ToString()
    {
        if (Promotion.HasValue)
            return $"{FromNotation}{ToNotation}{Promotion}";
        else
            return $"{FromNotation}{ToNotation}";
    }
}