using System;
using System.IO;
using System.Diagnostics;

public static class Logger
{
    private static readonly string logFilePath = @"C:\Users\jacob.thomas\Documents\Jacob Thomas_118577\Full Time 2024\Glidewell - BurQC\burinspection_fixturetransfertab_database\LogFolder\FixtureTransfer_ErrorLog.txt";

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
