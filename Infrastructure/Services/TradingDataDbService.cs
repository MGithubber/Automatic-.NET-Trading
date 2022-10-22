using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Infrastructure.Models;

using SqlConnection = System.Data.SqlClient.SqlConnection;

namespace AutomaticDotNETtrading.Infrastructure.Data;

public class TradingDataDbService : ITradingDataDbService<TVCandlestick>
{
    private readonly SqlDatabaseConnectionFactory ConnectionFactory;
    private SqlConnection? Connection;
    
    public TradingDataDbService(string ConnectionString, string DatabaseName) => this.ConnectionFactory = new (ConnectionString, DatabaseName);
    public TradingDataDbService(SqlDatabaseConnectionFactory connectionFactory) => this.ConnectionFactory = connectionFactory;

    //// //// ////
    
    public bool AddCandlestick(TVCandlestick candlestick)
    {
        this.Connection = this.ConnectionFactory.CreateConnection();
        
        using SqlCommand command = new SqlCommand("spAddCandlestick", this.Connection);
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@DateTime", candlestick.Date);
        command.Parameters.AddWithValue("@Open", candlestick.Open);
        command.Parameters.AddWithValue("@Close", candlestick.Close);
        command.Parameters.AddWithValue("@High", candlestick.High);
        command.Parameters.AddWithValue("@Low", candlestick.Low);

        bool success = command.ExecuteNonQuery() == 1;
        
        this.Connection.Close();
        
        return success;
    }
}
