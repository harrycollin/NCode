using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using NCode.Utilities;

namespace NCode
{
    public class DatabaseConnection
    {

        public static MySqlConnection connect1 = null;
        public static MySqlConnection connect2 = null;
        public static MySqlConnection connect3 = null;
        public static MySqlConnection connect4 = null;

        public static string ConnectionString;

        /// <summary>
        /// Used to test the connection to the Database. 
        /// </summary>
        /// <returns></returns>
        public static bool ConnectionTester()
        {   
            bool result = false;
            try
            {
                connect1.Open();            
            }
            catch (Exception e)
            {
                Tools.Print("Database connection failed", Tools.MessageType.error, e);
                result = false;
            }
            if (connect1.State == ConnectionState.Open)
            {
                result = true;
            }
            connect1.Close();           
            return result;
        }

        /// <summary>
        /// Initializes the connection to the Database.
        /// </summary>
        /// <param name="DatabaseConnectionString"></param>
        public static void ConnectionsInit(string DatabaseConnectionString)
        {
            ConnectionString = DatabaseConnectionString;

            connect1 = new MySqlConnection(DatabaseConnectionString);
            connect2 = new MySqlConnection(DatabaseConnectionString);
            connect3 = new MySqlConnection(DatabaseConnectionString);
            connect4 = new MySqlConnection(DatabaseConnectionString);
        }

        /// <summary>
        /// Used to select a closed connection to the database.
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection ConnectionChooser() 
        {
            MySqlConnection SelectedConnection = null;
            if (DatabaseConnection.connect1.State == ConnectionState.Closed) { SelectedConnection = connect1; }
            else if (DatabaseConnection.connect2.State == ConnectionState.Closed) { SelectedConnection = connect2; }
            else if (DatabaseConnection.connect3.State == ConnectionState.Closed) { SelectedConnection = connect3; }
            else if (DatabaseConnection.connect4.State == ConnectionState.Closed) { SelectedConnection = connect4; }

            return SelectedConnection;
        }

    }

    /// <summary>
    /// Default class for any requests.
    /// </summary>
    public class DatabaseRequest
    {
        /// <summary>
        /// Inserts a player into the database.
        /// </summary>
        /// <param name="playerInfo"></param>
        public static void NewEntry(PlayerInfo playerInfo)
        {
            MySqlConnection ChosenConnection = null;

            while (ChosenConnection == null)
            {
                ChosenConnection = DatabaseConnection.ConnectionChooser();

            }
            try
            {
                ChosenConnection.Open();
            }
            catch (MySqlException ex)
            {
                Tools.Print(ex.ToString());
            }

            using (MySqlCommand comm = new MySqlCommand())
            {
                comm.Connection = ChosenConnection;
                comm.CommandText = @"INSERT INTO players (steamid, info) VALUES (@var1, @var2); ";
                comm.Parameters.AddWithValue("@var1", playerInfo.steamid);
                comm.Parameters.AddWithValue("@var2", Converters.ConvertObjectToByteArray(playerInfo));
                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Tools.Print(ex.ToString());
                }
                ChosenConnection.Close();               
            }
        }

        /// <summary>
        /// Returns the PlayerInfo data for a select player.
        /// </summary>
        /// <param name="playerInfo"></param>
        /// <returns></returns>
        public static PlayerInfo GetEntry(PlayerInfo playerInfo)
        {
            MySqlConnection ChosenConnection = null;
            while (ChosenConnection == null)
            {
                ChosenConnection = DatabaseConnection.ConnectionChooser();
            }
            try
            {
                ChosenConnection.Open();
            }
            catch (MySqlException ex)
            {
                Tools.Print(ex.ToString());
            }

            using (MySqlCommand comm = new MySqlCommand())
            {
                comm.Connection = ChosenConnection;
                comm.CommandText = @"SELECT info FROM players WHERE steamid = @var1;";
                comm.Parameters.AddWithValue("@var1", playerInfo.steamid);
                try
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(comm);

                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    if (dt != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            byte[] ibytes = (byte[])dt.Rows[i]["info"];
                            PlayerInfo obj = (PlayerInfo)Converters.ConvertByteArrayToObject(ibytes);
                            ChosenConnection.Close();
                            return obj;
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Tools.Print(ex.ToString());
                }
                ChosenConnection.Close();
                return null;
            }
        }

        /// <summary>
        /// Overwrites a existing player's info in the database. 
        /// </summary>
        /// <param name="playerInfo"></param>
        /// <returns></returns>
        public static void SaveEntry(PlayerInfo playerInfo)
        {
            MySqlConnection ChosenConnection = null;
            while (ChosenConnection == null)
            {
                ChosenConnection = DatabaseConnection.ConnectionChooser();
            }
            try
            {
                ChosenConnection.Open();
            }
            catch (MySqlException ex)
            {
                Tools.Print(ex.ToString());
            }

            using (MySqlCommand comm = new MySqlCommand())
            {
                comm.Connection = ChosenConnection;
                comm.CommandText = @"UPDATE players SET info = @var1 WHERE steamid = @var2;";
                comm.Parameters.AddWithValue("@var1", Converters.ConvertObjectToByteArray(playerInfo));
                comm.Parameters.AddWithValue("@var2", playerInfo.steamid);
                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Tools.Print(ex.ToString());
                }
                ChosenConnection.Close();
            }
        }

        /// <summary>
        /// Checks whether a player exists in the database.
        /// </summary>
        /// <param name="steamid"></param>
        /// <returns></returns>
        public static bool DoesEntryExist(string steamid)
        {
            MySqlConnection ChosenConnection = null;
            while (ChosenConnection == null)
            {
                ChosenConnection = DatabaseConnection.ConnectionChooser();
            }
            try
            {
                ChosenConnection.Open();
            }
            catch (MySqlException ex)
            {
                Tools.Print(ex.ToString());
            }

            using (MySqlCommand comm = new MySqlCommand())
            {
                bool exists = false;
                comm.Connection = ChosenConnection;
                comm.CommandText = @"SELECT COUNT(*) FROM players WHERE steamid = @var1;";
                comm.Parameters.AddWithValue("@var1", steamid);
                try
                {
                    MySqlDataReader reader = comm.ExecuteReader();
                    reader.Read();
                    bool res = reader.GetBoolean(0);
                    if (res)
                    {
                        exists = true;
                    }
                }
                catch (MySqlException ex)
                {
                    Tools.Print(ex.ToString());
                }
                ChosenConnection.Close();
                return exists;
            }
        }

        /// <summary>
        /// Loads all objects from the Database.
        /// </summary>
        /// <returns></returns>
        public static HashSet<NetworkObject> LoadObjects()
        {
            MySqlConnection ChosenConnection = null;
            while (ChosenConnection == null)
            {
                ChosenConnection = DatabaseConnection.ConnectionChooser();
            }
            try
            {
                ChosenConnection.Open();
            }
            catch (MySqlException ex)
            {
                Tools.Print(ex.ToString());
            }

            HashSet<NetworkObject> list = new HashSet<NetworkObject>();

            using (MySqlCommand comm = new MySqlCommand())
            {
                comm.Connection = ChosenConnection;
                comm.CommandText = @"SELECT object FROM objects;";                        
                try
                {
                    MySqlDataAdapter da = new MySqlDataAdapter(comm);

                    DataTable dt = new DataTable();

                    da.Fill(dt);               

                    if (dt != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            byte[] ibytes = (byte[])dt.Rows[i]["object"];
                            NetworkObject obj = (NetworkObject)Converters.ConvertByteArrayToObject(ibytes);
                            list.Add(obj);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Tools.Print(ex.ToString());
                }
                ChosenConnection.Close();
            }
            return list;
        }

        /// <summary>
        /// Used to save or update any object to the database. Only to be referenced by Objectmanager.SaveObject().
        /// </summary>
        /// <param name="obj"></param>
        public static void SaveNewObject(NetworkObject obj)
        {
            MySqlConnection ChosenConnection = null;
            while (ChosenConnection == null)
            {
                ChosenConnection = DatabaseConnection.ConnectionChooser();
            }
            try
            {
                ChosenConnection.Open();
            }
            catch (MySqlException ex)
            {
                Tools.Print(ex.ToString());
            }


            using (MySqlCommand comm2 = new MySqlCommand())
            {
                comm2.Connection = ChosenConnection;
                comm2.CommandText = @"INSERT INTO objects (guid,object) VALUES (@var1,@var2);";
                comm2.Parameters.AddWithValue("@var1", obj.GUID);
                comm2.Parameters.AddWithValue("@var2", Converters.ConvertObjectToByteArray(obj));
                try
                {

                    comm2.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Tools.Print(ex.ToString());
                }
            }
            ChosenConnection.Close();           
        }

        /// <summary>
        /// Overwrites a existing object in the Database.
        /// </summary>
        /// <param name="obj"></param>
        public static void SaveExistingObject(NetworkObject obj)
        {
            MySqlConnection ChosenConnection = null;
            while (ChosenConnection == null)
            {
                ChosenConnection = DatabaseConnection.ConnectionChooser();
            }
            try
            {
                ChosenConnection.Open();
            }
            catch (MySqlException ex)
            {
                Tools.Print(ex.ToString());
            }

            using (MySqlCommand comm1 = new MySqlCommand())
            {
                comm1.Connection = ChosenConnection;
                comm1.CommandText = @"UPDATE objects SET object = @var1 WHERE guid = @var2;";
                comm1.Parameters.AddWithValue("@var1", Converters.ConvertObjectToByteArray(obj));
                comm1.Parameters.AddWithValue("@var2", obj.GUID);
                try
                {
                    comm1.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    Tools.Print(ex.ToString());
                }
            }


            ChosenConnection.Close();
        }

        /// <summary>
        /// Deletes a specific object from the database. Only to be referenced by Objectmanager.DeleteObject().
        /// </summary>
        /// <param name="obj"></param>
        public static void DeleteObject(string GUID)
        {
            MySqlConnection ChosenConnection = null;
            while (ChosenConnection == null)
            {
                ChosenConnection = DatabaseConnection.ConnectionChooser();
            }
            try
            {
                ChosenConnection.Open();
            }
            catch (MySqlException ex)
            {
                Tools.Print(ex.ToString());
            }

            using (MySqlCommand comm = new MySqlCommand())
            {
                comm.Connection = ChosenConnection;
                comm.CommandText = @"DELETE FROM objects WHERE guid = @var1";
                comm.Parameters.AddWithValue("@var1", GUID);
                try
                {
                    comm.ExecuteNonQuery();     
                }
                catch (MySqlException ex)
                {
                    Tools.Print(ex.ToString());
                }
            }
            ChosenConnection.Close();

        }
    }
}
