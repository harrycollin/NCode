using NCode.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCode
{
    class NCodeServerMain : NMainThread
    {
        /// <summary>
        /// Server Entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string systemPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            

            if (!Directory.Exists(Path.Combine(systemPath, "Config")))
            {
                try { Directory.CreateDirectory(Path.Combine(systemPath, "Config")); }catch (Exception e) { Tools.Print("Error creating directory - /Config", Tools.MessageType.error, e); }
            }
            if (!Directory.Exists(Path.Combine(systemPath, "Logs")))
            {
                try { Directory.CreateDirectory(Path.Combine(systemPath, "Logs")); } catch (Exception e) { Tools.Print("Error creating directory - /Logs", Tools.MessageType.error, e); }
            }
            if (!File.Exists(Path.Combine(systemPath, "Config/server.cfg")))
            {
                try { File.WriteAllText(Path.Combine(systemPath, "Config/server.cfg"), ConfigWriter()); } catch (Exception e) { Tools.Print("Error writing - Config/server.cfg", Tools.MessageType.error, e); }
            }

            string[] lines = null;
            try { lines = File.ReadAllLines(Path.Combine(systemPath, "Config/server.cfg")); } catch (Exception e) { Tools.Print("Unable to access the server.cfg", Tools.MessageType.error, e); }
            
            foreach(string i in lines)
            {
                char[] chars = { '"', ';' ,'=',' ' };
                if (i.StartsWith("server_name"))
                {
                    
                    string s = i.Substring(11);
                    string[] ss = s.Split(chars);
                    foreach(string i1 in ss) { Tools.Print(i1); }
                }   
            }


            NCodeServerMain app = new NCodeServerMain();
            app.Start();
        }


        public bool Start()
        {
            base.Start(5127);
            return false;
        }

        public static string ConfigWriter()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("----NCode Server Configuration---- v0.000.1 a --");
            builder.Append(System.Environment.NewLine);
            builder.Append("--Server Configuration--");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_name = \"\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_password = \"\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_tcpport = \"\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_udpport = \"\";");
            builder.Append(System.Environment.NewLine);
            builder.Append(System.Environment.NewLine);
            builder.Append("--Database Configuration--");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_ip = \"\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_port = \"\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_name = \"\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_user = \"\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_password = \"\";");


            return builder.ToString();
        }

        
    }
}
