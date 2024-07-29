using System;
using System.IO;
using System.Diagnostics;

public static class Logger
{
    private static readonly string logFilePath = "errorlog.txt"; // Path to your log file

    public static void LogError(string message, Exception ex)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine("Date : " + DateTime.Now.ToString());
                writer.WriteLine("Message : " + message);
                writer.WriteLine("Exception : " + ex.ToString());
                writer.WriteLine(new string('-', 80));
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine("Failed to log error: " + e.ToString());
        }
    }
}
