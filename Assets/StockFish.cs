using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class StockFish : MonoBehaviour
{
    private Process _stockfishProcess;

    [SerializeField]
    private string _stockfishExecutablePath = "";

    void Start()
    {
        if (string.IsNullOrEmpty(_stockfishExecutablePath))
        {
            _stockfishExecutablePath = Application.dataPath + "/Stockfish/stockfish.exe";
        }

        if (!File.Exists(_stockfishExecutablePath))
        {
            UnityEngine.Debug.LogError($"Stockfish executable not found at path: {_stockfishExecutablePath}");
            return;
        }

        InitializeStockfish();
    }

    void OnDestroy()
    {
        TerminateStockfish();
    }

    private void InitializeStockfish()
    {
        _stockfishProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _stockfishExecutablePath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        try
        {
            _stockfishProcess.Start();
            UnityEngine.Debug.Log("Stockfish started successfully.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to start Stockfish: {e.Message}");
        }
    }

    public List<Vector2Int> GetPiecesWithLegalMoves(string fen)
    {
        if (_stockfishProcess == null || _stockfishProcess.HasExited)
        {
            UnityEngine.Debug.LogError("Stockfish is not running!");
            return null;
        }

        StreamWriter inputWriter = _stockfishProcess.StandardInput;
        StreamReader outputReader = _stockfishProcess.StandardOutput;

        inputWriter.WriteLine("uci");
        inputWriter.WriteLine($"position fen {fen}");
        inputWriter.WriteLine("d");

        inputWriter.Flush();

        List<Vector2Int> piecesWithLegalMoves = new List<Vector2Int>();
        string line;
        while ((line = outputReader.ReadLine()) != null)
        {
            if (line.StartsWith("Legal moves:"))
            {
                string[] moves = line.Replace("Legal moves:", "").Trim().Split(' ');
                foreach (string move in moves)
                {
                    string position = move.Substring(0, 2); // Extract the piece's position (e.g., "e2")
                    Vector2Int gridPosition = ChessNotationToGrid(position);
                    if (!piecesWithLegalMoves.Contains(gridPosition))
                    {
                        piecesWithLegalMoves.Add(gridPosition);
                    }
                }
                break;
            }
        }

        return piecesWithLegalMoves;
    }

    public List<Vector2Int> GetLegalMovesForPiece(string fen, string piecePosition)
    {
        if (_stockfishProcess == null || _stockfishProcess.HasExited)
        {
            UnityEngine.Debug.LogError("Stockfish is not running!");
            return null;
        }

        StreamWriter inputWriter = _stockfishProcess.StandardInput;
        StreamReader outputReader = _stockfishProcess.StandardOutput;

        inputWriter.WriteLine("uci");
        inputWriter.WriteLine($"position fen {fen}");
        inputWriter.WriteLine("d");

        inputWriter.Flush();

        List<Vector2Int> legalMovesForPiece = new List<Vector2Int>();
        string line;
        while ((line = outputReader.ReadLine()) != null)
        {
            if (line.StartsWith("Legal moves:"))
            {
                string[] moves = line.Replace("Legal moves:", "").Trim().Split(' ');
                foreach (string move in moves)
                {
                    if (move.StartsWith(piecePosition)) // Match moves starting with the piece's position
                    {
                        string targetPosition = move.Substring(2, 2); // Extract target position
                        Vector2Int gridPosition = ChessNotationToGrid(targetPosition);
                        legalMovesForPiece.Add(gridPosition);
                    }
                }
                break;
            }
        }

        return legalMovesForPiece;
    }

    private Vector2Int ChessNotationToGrid(string position)
    {
        if (position.Length != 2) return new Vector2Int(-1, -1);

        int col = position[0] - 'a'; // Convert 'a' to 'h' into 0 to 7
        int row = position[1] - '1'; // Convert '1' to '8' into 0 to 7 (bottom to top)
        return new Vector2Int(row, col);
    }

    private void TerminateStockfish()
    {
        if (_stockfishProcess != null && !_stockfishProcess.HasExited)
        {
            _stockfishProcess.StandardInput.WriteLine("quit");
            _stockfishProcess.WaitForExit();
            _stockfishProcess.Close();
        }
    }
}
