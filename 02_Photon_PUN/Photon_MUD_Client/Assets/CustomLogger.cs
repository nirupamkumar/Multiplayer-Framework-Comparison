using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class CustomLogger
{
    private static string logFilePath = Application.persistentDataPath + "/PhotonGameLogs.txt";

    public static void WriteLog(string message)
    {
        string logMessage = $"{System.DateTime.Now}: {message}";

        Debug.Log(logMessage); // Log to Unity Console

        // Write to a log file
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine(logMessage);
        }
    }

    public static void ClearLog()
    {
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath); // Clear the existing log file
        }
    }
}
