using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Logger
{
    private static string logFilePath = "Logs/NetcodeGameLogs.txt"; // Path to the log file

    static Logger()
    {
        // Create the directory if it doesn't exist
        string directoryPath = Path.GetDirectoryName(logFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Initialize the log file
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"Log initialized at {DateTime.Now}");
        }
    }

    public static void Log(string message)
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    public static void LogPlayerAction(ulong clientId, string action)
    {
        Log($"Player {clientId}: {action}");
    }

    public static void LogChatMessage(ulong clientId, string message)
    {
        Log($"Player {clientId} says: {message}");
    }
}