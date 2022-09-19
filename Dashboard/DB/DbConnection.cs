using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;

namespace Dashboard.DB
{
    public abstract class DbConnection
    {
        private readonly string ConnectionString;

        public DbConnection()
        {
            ConnectionString = "Server=.;Database=NorthwindStore;User Id=sa;Password=Bimser123;";
            //ConnectionString = "Server=.; Database=NorthwindStore; Password=Bimser123; User=sa; User Id=sa; Integrated Security=true"; 

        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
       
    }
}
