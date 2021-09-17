using System.Data.SqlClient;

namespace InsercionMasiva.Models
{
    class sqlConnection
    {
        public static SqlConnection getConnection()
        {
            return new SqlConnection(@"Server = SISTEMAS-34\SQLEXPRESS; Database = MaximosyMinimos; User Id = sa; Password = 123;");
        }

    }
}
