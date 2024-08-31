using System;
using System.Configuration;
using System.IO;
namespace ChatServer
{
    public static class MyLogger
    {
        public static void WriteLog(string message)
        {
            string logPath = ConfigurationManager.AppSettings["logPath"];
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine($"{DateTime.Now} : {message}");
            }
        }
    }
}