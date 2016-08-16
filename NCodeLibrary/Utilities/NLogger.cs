using System;
using System.Collections;
using System.IO;


namespace NCode.Utilities
{
    public static class NLogger
    {
        static string pathToSessionLog;
        static Queue toLog = new Queue();
        static int lineNumber = 0;

        public static void LogToFile(string message = null)
        {
            if (message != null)
                toLog.Enqueue(message);
            if (pathToSessionLog == null)
                CreateNewLog();
            if (toLog.Count > 0)
            {
                try
                {
                    using (StreamWriter file =
                    new StreamWriter(pathToSessionLog, true))
                    {
                        file.Write(lineNumber++.ToString() + ": " + toLog.Peek());
                    }
                }
                catch (Exception e)
                {
                    LogToFile();
                    return;
                }
                toLog.Dequeue();
            }
            if (toLog.Count > 0)
                LogToFile();
        }


        static void CreateNewLog()
        {
            try
            {
                string time = "server_activity_["+
                    DateTime.Now.Day.ToString() + "-" +
                    DateTime.Now.Month.ToString() + "-" +
                    DateTime.Now.Year.ToString() + "]_[" +
                    DateTime.Now.Hour.ToString() + "." +
                    DateTime.Now.Minute.ToString() + "]" +
                     ".log";

                pathToSessionLog = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Logs/" + time;
                FileStream stream = File.Create(pathToSessionLog);
                stream.Close();

            }
            catch (Exception e)
            {
                Tools.Print("", Tools.MessageType.error, e);
            }
        }
    }
}
