using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections;

namespace Erosionlunar.ProcesadorLibros.DB
{
    public class DBConector
    {
        private readonly MySqlConnection conexionDB;

        public DBConector()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            conexionDB = new MySqlConnection(connectionString);
        }
        public void Dispose()
        {
            conexionDB?.Dispose();
        }
        /// <summary>
        /// Executes INSERT, UPDATE, DELETE on the database.
        /// </summary>
        /// <param name="query">The SQL query string to execute.</param>
        /// <param name="paramQuery">List of parameter names in the query.</param>
        /// <param name="valuesParam">List of values corresponding to the query parameters.</param>
        /// <exception cref="MySqlException">Thrown if an error occurs during the database operation.</exception>
        /// <remarks>
        /// This method ensures that query parameters are safely added and manages 
        /// the connection lifecycle using a `using` block to dispose resources.
        /// </remarks>
        public void WriteQuery(string query, List<string> paramQuery, List<string> valuesParam)//INSERT, UPDATE, DELETE
        {
            try
            {
                using (MySqlCommand cmdDB = new MySqlCommand(query, conexionDB))        
                {
                    for (int i = 0; i < paramQuery.Count; i++) // adds parameters to the query
                    {
                        cmdDB.Parameters.AddWithValue(paramQuery[i], valuesParam[i]);
                    }
                    conexionDB.Open();
                    cmdDB.ExecuteNonQuery();// tries execute the command
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }
            finally
            {
                conexionDB.Close();
            }
        }
        /// <summary>
        /// Brings a list of strings of the query response not using parameters.
        /// </summary>
        /// <param name="query">The SQL query string to execute.</param>
        /// <param name="columnName">Name of the column of the select of the query.</param>
        /// <remarks>
        /// This method does a SELECT query to de DB
        /// and brings a list with only one selected column.
        /// </remarks>
        public List<string> readQuerySimple(string query, string columnName)
        {
            var theResponse = new List<string>();
            try
            {
                using (MySqlCommand cmdDB = new MySqlCommand(query, conexionDB))
                {
                    conexionDB.Open();
                    var reader = cmdDB.ExecuteReader();   //tries to execute the select command
                    while (reader.Read())      // gets the results from the query
                    {
                        theResponse.Add(reader[columnName].ToString());
                    }
                    reader.Close();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }
            finally
            {
                conexionDB.Close();
            }

            return theResponse;
        }
        /// <summary>
        /// Brings a list of strings of the query response.
        /// </summary>
        /// <param name="query">The SQL query string to execute.</param>
        /// <param name="columnName">Name of the column of the select of the query.</param>
        /// <param name="paramQuery">List of strings corresponding to the query parameters.</param>
        /// <param name="valuesParam"> List of strings corresponding to the parameters values.</param>
        /// <remarks>
        /// This method does a SELECT query to de DB
        /// and brings a list with only one selected column.
        /// </remarks>
        public List<string> readQuerySimple(string query, List<string> paramQuery, List<string> valuesParam, string columnName)
        {
            var theResponse = new List<string>();
            try
            {
                using (MySqlCommand cmdDB = new MySqlCommand(query, conexionDB))             
                {
                    for (int i = 0; i < paramQuery.Count; i++) //adds the parameters
                    {
                        cmdDB.Parameters.AddWithValue(paramQuery[i], valuesParam[i]);
                    }
                    conexionDB.Open();
                    var reader = cmdDB.ExecuteReader();   //tries to execute the select command
                    while (reader.Read())      // gets the results from the query
                    {
                        theResponse.Add(reader[columnName].ToString());
                    }
                    reader.Close();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }
            finally
            {
                conexionDB.Close();
            }

            return theResponse;
        }
        /// <summary>
        /// Brings a list of list of strings of the query response.
        /// </summary>
        /// <param name="query">The SQL query string to execute.</param>
        /// <param name="columnNames">Name of the columns of the select of the query.</param>
        /// <param name="paramQuery">List of strings corresponding to the query parameters.</param>
        /// <param name="valuesParam"> List of strings corresponding to the parameters values.</param>
        /// <remarks>
        /// This method does a SELECT query to de DB
        /// and brings a list with for every row.
        /// </remarks>
        public List<List<string>> readQueryList(string query, List<string> paramQuery, List<string> valuesParam, List<string> columnNames)
        {
            var theResponse = new List<List<string>>();
            try
            {
                using (MySqlCommand cmdDB = new MySqlCommand(query, conexionDB)) 
                {
                    for (int i = 0; i < paramQuery.Count; i++) //adds the parameters
                    {
                        cmdDB.Parameters.AddWithValue(paramQuery[i], valuesParam[i]);
                    }
                    conexionDB.Open();
                    var reader = cmdDB.ExecuteReader(); //tries to execute the select command

                    while (reader.Read())
                    {
                        var rowResponse = new List<string>();
                        foreach (string name in columnNames)
                        {
                            rowResponse.Add(reader[name].ToString());
                        }
                        theResponse.Add(rowResponse);
                    }
                    reader.Close();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }
            finally
            {
                conexionDB.Close();
            }

            return theResponse;
        }
        /// <summary>
        /// Brings a list of list of strings of the query response without parameters.
        /// </summary>
        /// <param name="query">The SQL query string to execute.</param>
        /// <param name="columnNames">Name of the columns of the select of the query.</param>
        /// <remarks>
        /// This method does a SELECT query to de DB
        /// and brings a list with for every row without parameters.
        /// </remarks>
        public List<List<string>> readQueryList(string query, List<string> columnNames)
        {
            var theResponse = new List<List<string>>();
            try
            {
                using (MySqlCommand cmdDB = new MySqlCommand(query, conexionDB))
                {
                    conexionDB.Open();
                    var reader = cmdDB.ExecuteReader(); //tries to execute the select command

                    while (reader.Read())
                    {
                        var rowResponse = new List<string>();
                        foreach (string name in columnNames)
                        {
                            rowResponse.Add(reader[name].ToString());
                        }
                        theResponse.Add(rowResponse);
                    }
                    reader.Close();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }
            finally
            {
                conexionDB.Close();
            }

            return theResponse;
        }
    }
}
