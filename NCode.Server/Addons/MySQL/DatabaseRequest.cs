using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using NCode.Core.Utilities;

namespace NCode.Server.Addons.MySQL
{
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
                comm.Parameters.AddWithValue("@var2", NConverters.ConvertObjectToByteArray(playerInfo));
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
                            PlayerInfo obj = (PlayerInfo)NConverters.ConvertByteArrayToObject(ibytes);
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
                comm.Parameters.AddWithValue("@var1", NConverters.ConvertObjectToByteArray(playerInfo));
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
                            NetworkObject obj = (NetworkObject)NConverters.ConvertByteArrayToObject(ibytes);
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
                comm2.Parameters.AddWithValue("@var2", NConverters.ConvertObjectToByteArray(obj));
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
                comm1.Parameters.AddWithValue("@var1", NConverters.ConvertObjectToByteArray(obj));
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