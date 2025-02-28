using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class StockFish : MonoBehaviour
{
    private Process stockfishProcess;

    // Path to the Stockfish executable
    private string stockfishPath = Application.dataPath + "/Stockfish/stockfish.exe";


    void Start()
    {
        StartStockfish();
    }

    void OnDestroy()
    {
        CloseStockfish();
    }

    private void StartStockfish()
    {
        stockfishProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = stockfishPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        stockfishProcess.Start();
    }

    public string GetBestMove(string fen)
    {
        if (stockfishProcess == null || stockfishProcess.HasExited)
        {
            UnityEngine.Debug.LogError("Stockfish is not running!");
            return null;
        }

        // Send UCI commands to Stockfish
        StreamWriter inputWriter = stockfishProcess.StandardInput;
        StreamReader outputReader = stockfishProcess.StandardOutput;

        inputWriter.WriteLine("uci");
        inputWriter.WriteLine($"position fen {fen}");
        inputWriter.WriteLine("go depth 20"); // Adjust depth as needed

        inputWriter.Flush();

        // Read the best move from Stockfish's output
        string bestMove = null;
        while (true)
        {
            string line = outputReader.ReadLine();
            if (line.StartsWith("bestmove"))
            {
                bestMove = line.Split(' ')[1]; // Extract best move
                break;
            }
        }

        return bestMove;
    }

    private void CloseStockfish()
    {
        if (stockfishProcess != null && !stockfishProcess.HasExited)
        {
            stockfishProcess.StandardInput.WriteLine("quit");
            stockfishProcess.WaitForExit();
            stockfishProcess.Close();
        }
    }
}
