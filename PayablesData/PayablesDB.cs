using System.Data.SqlClient;
using System.Configuration;

namespace PayablesData
{
    public static class PayablesDB
    {
        public static SqlConnection GetConnection()
        {
            string connectionString =  ConfigurationManager.ConnectionStrings["PayablesConnectionString"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }
    }
}
