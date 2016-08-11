using System;
using System.IO;


namespace NCode.Utilities
{
    public static class NLogger
    {
        static string pathToSessionLog;
        public static void LogToFile(string message)
        {
            if(pathToSessionLog == null)
            {
                CreateNewLog();
            }
            File.AppendAllText(pathToSessionLog, "[" + DateTime.Now + "]: " + message);

        }
        
        static void CreateNewLog()
        {
            try
            {
                string time = 
                    DateTime.Now.Hour.ToString() + "." +
                    DateTime.Now.Minute.ToString() + "_" +
                    DateTime.Now.Day.ToString() + "-" +
                    DateTime.Now.Month.ToString() + "-" +
                    DateTime.Now.Year.ToString() + ".log";

                pathToSessionLog = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Logs/" + time;
                FileStream stream = File.Create(pathToSessionLog);
                stream.Close();

            }catch(Exception e)
            {
                Tools.Print("", Tools.MessageType.error, e);
            }
        }
    }
}
