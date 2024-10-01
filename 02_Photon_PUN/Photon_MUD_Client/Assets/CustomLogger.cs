using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CustomLogger: MonoBehaviour
{
    private static CustomLogger _instance;
    private string logFilePath;

    public static CustomLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CustomLogger>();

                if (_instance == null)
                {
                    GameObject loggerObject = new GameObject("Logger");
                    _instance = loggerObject.AddComponent<CustomLogger>();
                    DontDestroyOnLoad(loggerObject);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            logFilePath = Path.Combine(Application.persistentDataPath, "GameLog.txt");
            File.WriteAllText(logFilePath, "Game Log Started at " + System.DateTime.Now + "\n");
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Log(string message)
    {
        string timeStampedMessage = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] " + message;
        Debug.Log(timeStampedMessage); 

        try
        {
            File.AppendAllText(logFilePath, timeStampedMessage + "\n");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Logger: Failed to write to log file. Exception: " + e.Message);
        }
    }

    public void ClearLog()
    {
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }
    }
}
