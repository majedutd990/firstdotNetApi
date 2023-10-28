using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data
{
    public class DataContextDapper
    {
        private readonly IConfiguration _config;

        public DataContextDapper(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            return dbConnection.Query<T>(sql);
        }

        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            return dbConnection.QuerySingle<T>(sql);
        }

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql) > 0;
        }

        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }

        public bool ExecuteSqlWithParams(string sql, List<SqlParameter> paramsList)
        {
            SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            SqlCommand commandWithParams = new SqlCommand(sql);
            foreach (SqlParameter parameter in paramsList)
            {
                commandWithParams.Parameters.Add(parameter);
            }

            dbConnection.Open();
            commandWithParams.Connection = dbConnection;
            int rowsEffected = commandWithParams.ExecuteNonQuery();
            dbConnection.Close();
            return rowsEffected > 0;
        }
    }
}