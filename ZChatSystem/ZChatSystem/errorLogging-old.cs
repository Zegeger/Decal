using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace ZChatSystem
{
    internal static class errorLogging
    {
        private static string errorLogFile = "d:\\errorLog.txt";

        internal static void LogError(Exception ex)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(errorLogFile, true);
            sw.WriteLine("============================================================================");
            sw.WriteLine(DateTime.Now.ToString());
            sw.WriteLine("Error: " + ex.Message);
            sw.WriteLine("Source: " + ex.Source);
            sw.WriteLine("Stack: " + ex.StackTrace);
            if (ex.InnerException != null)
            {
                sw.WriteLine("Inner: " + ex.InnerException.Message);
                sw.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
            }
            sw.WriteLine("============================================================================");
            sw.WriteLine("");
            sw.Close();
        }
    }
}