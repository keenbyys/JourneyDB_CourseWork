using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JourneyDB_CW
{
    class DataBaseProcedures
    {
        private readonly string _connectionString;

        public DataBaseProcedures(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable ExecuteStoredProcedure(string procedureName)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return dataTable;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database connection error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }
        }

        public DataTable ExecuteStoredProcedure(string procedureName, params MySqlParameter[] parameters)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (parameters != null && parameters.Length > 0)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return dataTable;
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"Database connection error: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred: {ex.Message}", ex);
                }
            }
        }
    }
}
