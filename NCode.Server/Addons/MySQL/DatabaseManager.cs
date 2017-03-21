using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace NCode.Server.Addons.MySQL
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
            catch (Exception ex)
            {
                //log e
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
}
