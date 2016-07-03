using System;
using System.Text;
#if UNITY_EDITOR
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
        static public void Print(string msg, MessageType type = MessageType.notification, Exception ex = null)
        {
            StringBuilder builder = new StringBuilder();
            DateTime now = DateTime.Now;
            builder.Append("[" + now + "]: ");
            switch (type)
            {
                case MessageType.warning: { builder.Append("Warning: "); break; }
                case MessageType.error: { builder.Append("Error: "); break; }
            }

            builder.Append(msg);
            builder.Append(Environment.NewLine);
            if(ex != null) { builder.Append("| Exception: " + ex.ToString()); }

#if UNITY_EDITOR
            Debug.Log(builder.ToString());
#else
            Console.Write(builder.ToString());
#endif

        }
    }
}
