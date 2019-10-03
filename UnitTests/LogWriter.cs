using System;
using System.IO;
using System.Reflection;

namespace Worker
{
    public class LogWriter
    {
        private static string m_exePath = string.Empty;

        public LogWriter(string logMessage)
        {
            LogWrite(logMessage);
        }
        public static void LogWrite(string logMessage)
        {
            m_exePath = Path.Combine(RunTest.logDirectoryPath, "logs");

            // Create the logs directory
            if (!Directory.Exists(m_exePath))
            {
                DirectoryInfo directory = Directory.CreateDirectory(m_exePath);
                if (!directory.Exists)
                {
                    throw new Exception(DateTime.Now + " Failed to create directory.");
                }
            }
            try
            {
                using (StreamWriter w = File.AppendText(Path.Combine(m_exePath, "log.txt")))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create Log: " + e.Message);
            }
        }

        public static void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : " + DateTime.Now + " : ");
                txtWriter.WriteLine(logMessage);
                txtWriter.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to write into Log: " + e.Message);
            }
        }
    }
}
