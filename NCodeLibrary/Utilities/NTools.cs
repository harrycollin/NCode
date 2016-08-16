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

        public static void Print(string msg, MessageType type = MessageType.notification, Exception e = null, bool Log = true, bool Print = true)
        {
            if (!Log && !Print) return;

            StringBuilder logBuilder = new StringBuilder();

            DateTime now = DateTime.Now;
            logBuilder.Append("[" + now + "]: ");

            //The message type here.
            switch (type)
            {
                case MessageType.warning: {  logBuilder.Append("[[WARNING]]: "); break; }
                case MessageType.error: {  logBuilder.Append("[[ERROR]]: "); break; }
            }
            
            logBuilder.Append(msg);
            logBuilder.Append(Environment.NewLine);
            if(e != null) { logBuilder.Append("| Exception: " + e.ToString()); }

            //Do we want to print this to the console?
            if (Print)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                Debug.Log(logBuilder.ToString());
#else
                Console.Write(logBuilder.ToString());
#endif
            }

            //We aren't logging in Unity yet.
#if !UNITY_EDITOR && !UNITY_STANDALONE
            if (Log) 
            { 
                NLogger.LogToFile(logBuilder.ToString()); 
            }
#endif
        }
    }
}
