using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace JourneyDB_CW
{
    class DataBaseHelper
    {
        private string connectionString = "Server=localhost;Database=db_joint_journey;Uid=root;Pwd=50026022SVK-23;";

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
                    System.Windows.MessageBox.Show($"Помилка при з'єднанні з БД: {ex.Message}");
                }
            }

            return dataTable;
        }
    }
}
