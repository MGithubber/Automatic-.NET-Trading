using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticDotNETtrading.Application.Interfaces.Data;
using SqlConnection = System.Data.SqlClient.SqlConnection;

namespace AutomaticDotNETtrading.Infrastructure.Data;

public class SqlDatabaseConnectionFactory : IDatabaseConnectionFactory<SqlConnection>
{
    private readonly string ConnectionString;

    public SqlDatabaseConnectionFactory(string ConnectionString) => this.ConnectionString = ConnectionString;

    
    public SqlConnection CreateConnection()
    {
        try
        {
            SqlConnection SqlConnection = new SqlConnection(this.ConnectionString);
            SqlConnection.Open();
            return SqlConnection;
        }
        catch (Exception exception) { throw new Exception("Connection to database failed", exception); }
    }
}
