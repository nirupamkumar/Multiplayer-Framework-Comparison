using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Logger
{
    private static string logFilePath = "Logs/Netcode_GameLogs.txt"; // Path to the log file

    static Logger()
    {
        string directoryPath = Path.GetDirectoryName(logFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"Log initialized at {DateTime.Now}");
        }
    }

    public static void Log(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write log: {ex.Message}");
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
