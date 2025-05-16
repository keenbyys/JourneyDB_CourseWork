using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace JourneyDB_CW
{
    class DatabaseHelper
    {
        private string connectionString = "Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;";
        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public DataTable SelectQuery(string query)
        {
            DataTable dataTable = new DataTable();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand commandSelect = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapterSelect = new MySqlDataAdapter(commandSelect);
                    adapterSelect.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error connecting to the database: {ex.Message}");
                }
            }

            return dataTable;
        }

        public void NoneQuery(string query)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand commandUpdateInsertDelete = new MySqlCommand(query, connection);
                    commandUpdateInsertDelete.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error connecting to the database: {ex.Message}");
                }
            }
        }
    }
}
