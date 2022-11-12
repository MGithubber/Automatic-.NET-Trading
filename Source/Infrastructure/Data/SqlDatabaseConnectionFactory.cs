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
    public readonly string DatabaseName;
    
    public SqlDatabaseConnectionFactory(string ConnectionString, string DatabaseName)
    {
        this.ConnectionString = ConnectionString;
        this.DatabaseName = DatabaseName;
    }

    //////////
        
    public SqlConnection CreateConnection()
    {
        try
        {
            SqlConnection SqlConnection = new SqlConnection(this.ConnectionString);
            SqlConnection.Open();
            SqlConnection.ChangeDatabase(this.DatabaseName);
            return SqlConnection;
        }
        catch (Exception exception) { throw new Exception("Connection to database failed", exception); }
    }
}
