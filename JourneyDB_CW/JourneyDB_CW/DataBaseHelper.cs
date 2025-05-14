using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;

namespace JourneyDB_CW
{
    class DataBaseHelper
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
                    System.Windows.MessageBox.Show($"Помилка при з'єднанні з БД: {ex.Message}");
                }
            }

            return dataTable;
        }

        public void AddQuery(string query)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Фиксированная запись успешно добавлена!");
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при добавлении записи.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
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
                    System.Windows.MessageBox.Show($"Помилка при з'єднанні з БД: {ex.Message}");
                }
            }
        }

    }
}
