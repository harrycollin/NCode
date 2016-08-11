using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NCode
{
    class NCodeServerMain 
    {
        /// <summary>
        /// Server Entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            NLogger.LogToFile("Application launched\n");
            
            //The server's system path.
            string systemPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            //Checks all default files and folders are present.         
            CheckPaths(systemPath);

            //Read the config file
            string[] lines = null;
            try { lines = File.ReadAllLines(Path.Combine(systemPath, "Config/server.cfg")); } catch (Exception e) { Tools.Print("Unable to access the server.cfg", Tools.MessageType.error, e); }
            ConfigInfo info = ConfigReader(lines);

            //Initialize the database connections
            DatabaseConnection.ConnectionsInit("datasource = " + info.databaseip + "; database = " + info.databasename + "; port = " + info.databaseport + "; username = " + info.databaseuser + "; password = " + info.databasepassword + ";");
            //Test the connections
            if (!DatabaseConnection.ConnectionTester()) { Tools.Print("Failed to establish a connection to the database. Please check settings in 'server.cfg' and make sure all ports are forwarded", Tools.MessageType.error);  Console.ReadLine(); return; }

            //Makes a new instance of the server. 
            NMainThreads app = new NMainThreads();
            app.Start(info.servername, info.tcpport, info.udpport, info.rconport, info.password, info.rconpassword, info.autostart);

        }

        

        /// <summary>
        /// Checks the default server and file paths. Creates them if not present.
        /// </summary>
        /// <returns></returns>
        public static bool CheckPaths(string systemPath)
        {
            if (!Directory.Exists(Path.Combine(systemPath, "Config")))
            {
                try { Directory.CreateDirectory(Path.Combine(systemPath, "Config")); } catch (Exception e) { Tools.Print("Error creating directory - /Config", Tools.MessageType.error, e); return false; }
            }
            if (!Directory.Exists(Path.Combine(systemPath, "Logs")))
            {
                try { Directory.CreateDirectory(Path.Combine(systemPath, "Logs")); } catch (Exception e) { Tools.Print("Error creating directory - /Logs", Tools.MessageType.error, e); return false; }
            }
            if (!File.Exists(Path.Combine(systemPath, "Config/server.cfg")))
            {
                try { File.WriteAllText(Path.Combine(systemPath, "Config/server.cfg"), ConfigWriter()); } catch (Exception e) { Tools.Print("Error writing - Config/server.cfg", Tools.MessageType.error, e); return false; }
            }
            return true;
        }

        /// <summary>
        /// Writes a default config file to the /config directory
        /// </summary>
        /// <returns></returns>
        public static string ConfigWriter()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("----NCode Server Configuration---- v0.000.1 a --");
            builder.Append(System.Environment.NewLine);
            builder.Append("--Server Configuration--");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_name = \"kleos server\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_password = \"changeme\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_rconpassword = \"changeme\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_tcpport = \"5127\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_udpport = \"5128\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_rconport = \"5129\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("server_autostart = \"true\";");
            builder.Append(System.Environment.NewLine);
            builder.Append(System.Environment.NewLine);
            builder.Append("--Database Configuration--");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_ip = \"127.0.0.1\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_port = \"3306\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_name = \"kleos_data\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_user = \"root\";");
            builder.Append(System.Environment.NewLine);
            builder.Append("database_password = \"password\";");
            return builder.ToString();
        }

        /// <summary>
        /// Parses the parameters read from server.cfg
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static ConfigInfo ConfigReader(string[] lines)
        {
            ConfigInfo info = new ConfigInfo();

            foreach (string i in lines)
            {
                //Server name
                if (i.StartsWith("server_name"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.servername = item.ToString().Replace("\"", "");
                        }
                        catch (Exception e)
                        {
                            info.servername = "Kleos Server";
                            Tools.Print("Failed to read the 'server_name' parameter in 'server.cfg'. Defaulting server name to 'Kleos Server'.", Tools.MessageType.error, e, true);
                        }

                    }
                    continue;

                }

                //Server password
                if (i.StartsWith("server_password"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.password = item.ToString().Replace("\"", "");
                        }
                        catch (Exception e)
                        {
                            info.password = null;
                            Tools.Print("Failed to read the 'server_password' parameter in 'server.cfg'. Defaulting server server password to null.", Tools.MessageType.error, e, true);
                        }
                    }
                    continue;
                }
                //Server password
                if (i.StartsWith("server_rconpassword"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.rconpassword = item.ToString().Replace("\"", "");
                        }
                        catch (Exception e)
                        {
                            info.rconpassword = null;
                            Tools.Print("Failed to read the 'server_rconpassword' parameter in 'server.cfg'. Defaulting rcon server password to 'changeme'.", Tools.MessageType.error, e, true);
                        }
                    }
                    continue;
                }

                //Tcp Port
                if (i.StartsWith("server_tcpport"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.tcpport = int.Parse(item.ToString().Replace("\"", ""));
                        }
                        catch (Exception e)
                        {
                            info.tcpport = 5127;
                            Tools.Print("Failed to read the 'server_tcpport' parameter in 'server.cfg'. Defaulting TCP port to 5127.", Tools.MessageType.error, e, true);
                        }
                    }
                    continue;
                }

                //Udp Port
                if (i.StartsWith("server_udpport"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.udpport = int.Parse(item.ToString().Replace("\"", ""));
                        }
                        catch (Exception e)
                        {
                            info.udpport = 5128;
                            Tools.Print("Failed to read the 'server_udpport' parameter in 'server.cfg'. Defaulting UDP port to 5128.", Tools.MessageType.error, e, true);
                        }
                    }
                    continue;
                }

                //RCon Port
                if (i.StartsWith("server_rconport"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.rconport = int.Parse(item.ToString().Replace("\"", ""));
                        }
                        catch (Exception e)
                        {
                            info.rconport = 5129;
                            Tools.Print("Failed to read the 'server_rconport' parameter in 'server.cfg'. Defaulting RCon port to 5129.", Tools.MessageType.error, e, true);
                        }
                    }
                    continue;
                }
                //RCon Port
                if (i.StartsWith("server_autostart"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.autostart = bool.Parse(item.ToString().Replace("\"", ""));
                        }
                        catch (Exception e)
                        {
                            info.autostart = true;
                            Tools.Print("Failed to read the 'server_autostart' parameter in 'server.cfg'. Defaulting to true.", Tools.MessageType.error, e, true);
                        }
                    }
                    continue;
                }

                //Database ip
                if (i.StartsWith("database_ip"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.databaseip = item.ToString().Replace("\"", "");
                        }
                        catch (Exception e)
                        {
                            info.databaseip = "127.0.0.1";
                            Tools.Print("Failed to read the 'database_ip' parameter in 'server.cfg'. Defaulting to '127.0.0.1'.", Tools.MessageType.error, e, true);
                        }
                        

                    }
                    continue;

                }
                //Database Port
                if (i.StartsWith("database_port"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.databaseport = int.Parse(item.ToString().Replace("\"", ""));
                        }
                        catch (Exception e)
                        {
                            info.databaseport = 3306;
                            Tools.Print("Failed to read the 'database_port' parameter in 'server.cfg'. Defaulting port to 3306.", Tools.MessageType.error, e, true);
                        }


                    }
                    continue;

                }

                //Database user
                if (i.StartsWith("database_user"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.databaseuser = item.ToString().Replace("\"", "");
                        }
                        catch (Exception e)
                        {
                            info.databaseuser = "admin";
                            Tools.Print("Failed to read the 'database_user' parameter in 'server.cfg'. Defaulting to 'admin'.", Tools.MessageType.error, e, true);
                        }
                    }
                    continue;

                }
                //Database password
                if (i.StartsWith("database_password"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.databasepassword = item.ToString().Replace("\"", "");
                        }
                        catch (Exception e)
                        {
                            info.databasepassword = "changeme";
                            Tools.Print("Failed to read the 'database_password' parameter in 'server.cfg'. Defaulting to 'changeme'.", Tools.MessageType.error, e, true);
                        }
                    }
                    continue;

                }
                //Database name
                if (i.StartsWith("database_name"))
                {
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(i);
                    foreach (var item in matches)
                    {
                        try
                        {
                            info.databasename = item.ToString().Replace("\"", "");
                        }
                        catch (Exception e)
                        {
                            info.databasename = "kleos_data";
                            Tools.Print("Failed to read the 'database_name' parameter in 'server.cfg'. Defaulting to 'kleos_data'.", Tools.MessageType.error, e, true);
                        }

                    }
                    continue;
                }
            }
            return info;
        }
    }

    
    /// <summary>
    /// Local to this file class used to pack parameters. 
    /// </summary>
    class ConfigInfo
    {
        public string servername { get; set; }
        public string password { get; set; }
        public string rconpassword { get; set; }
        public int tcpport { get; set; }
        public int udpport { get; set; }
        public int rconport { get; set; }
        public bool autostart { get; set; }
        public string databaseip { get; set; }
        public int databaseport { get; set; }
        public string databasename { get; set; }
        public string databaseuser { get; set; }
        public string databasepassword { get; set; }
    }
}
