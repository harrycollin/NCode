using NCode.Utilities;
using System;
using System.IO;
using System.Text;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
#endif

namespace NCode
{
    public static class Tools
    {
        public enum MessageType
        {
            notification,
            warning,
            error,
        }

        /// <summary>
        /// Used to print various types of messages. 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        /// <param name="ex"></param>
        public static void Print(string msg, MessageType type = MessageType.notification, Exception ex = null, bool Log = false)
        {
            StringBuilder printBuilder = new StringBuilder();
            StringBuilder logBuilder = new StringBuilder();

            DateTime now = DateTime.Now;
            printBuilder.Append("[" + now + "]: ");
            logBuilder.Append("[" + now + "]: ");

            switch (type)
            {
                case MessageType.warning: { printBuilder.Append("Warning: "); logBuilder.Append("[[WARNING]]:"); break; }
                case MessageType.error: { printBuilder.Append("Error: "); logBuilder.Append("[[ERROR]]:"); break; }
            }

            printBuilder.Append(msg);
            printBuilder.Append(Environment.NewLine);
            logBuilder.Append(msg);
            logBuilder.Append(Environment.NewLine);
            if(ex != null) { printBuilder.Append("| Exception: " + ex.ToString()); }

#if UNITY_EDITOR|| UNITY_STANDALONE
            Debug.Log(printBuilder.ToString());
#else
            Console.Write(printBuilder.ToString());
#endif
            if (Log) { NLogger.LogToFile(logBuilder.ToString()); }
        }

        

        
    }
}
