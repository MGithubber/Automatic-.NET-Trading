using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlConnection = System.Data.SqlClient.SqlConnection;

using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Infrastructure.Models;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Enums;
using System.Diagnostics;
using AutomaticDotNETtrading.Domain.Models;
using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Infrastructure.Data;

public class TradingDataDbService : ITradingDataDbService<TVCandlestick>
{
    private readonly SqlDatabaseConnectionFactory ConnectionFactory;
    private SqlConnection? Connection;
    
    public TradingDataDbService(string ConnectionString, string DatabaseName) => this.ConnectionFactory = new (ConnectionString, DatabaseName);
    public TradingDataDbService(SqlDatabaseConnectionFactory connectionFactory) => this.ConnectionFactory = connectionFactory;

    //// //// ////
    
    public int AddCandlestick(TVCandlestick candlestick)
    {
        try
        {
            this.Connection = this.ConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand
            {
                CommandText = "spAddCandlestickIfNotExists",
                Connection = this.Connection,
                CommandType = CommandType.StoredProcedure,
            };

            #region SqlCommand parameters
            command.Parameters.AddWithValue("@CurrencyPair", candlestick.CurrencyPair.Name);
            command.Parameters.AddWithValue("@DateTime", candlestick.Date);
            command.Parameters.AddWithValue("@Open", candlestick.Open);
            command.Parameters.AddWithValue("@Close", candlestick.Close);
            command.Parameters.AddWithValue("@High", candlestick.High);
            command.Parameters.AddWithValue("@Low", candlestick.Low);
            command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@ScopeIdentity",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.ReturnValue,
            });
            #endregion
            
            
            int rows = command.ExecuteNonQuery();
            this.Connection.Close();
            
            if (rows < 1)
            {
                throw new ArgumentException($"A candlestick with {nameof(CurrencyPair)} == \"{candlestick.CurrencyPair.Name}\" and {nameof(candlestick.Date)} == \"{candlestick.Date}\" is already in the database", nameof(candlestick));
            }

            
            return (int)command.Parameters["@ScopeIdentity"].Value;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
    
    public void AddFuturesOrder(BinanceFuturesOrder FuturesOrder, TVCandlestick Candlestick, out int FuturesOrder_Id, out int Candlestick_Id)
    {
        try
        {
            this.Connection = this.ConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand
            {
                CommandText = "spAddFuturesOrderAndCandlestickIfNotExists",
                Connection = this.Connection,
                CommandType = CommandType.StoredProcedure,
            };

            #region SqlCommand parameters
            command.Parameters.AddWithValue("@CurrencyPair", Candlestick.CurrencyPair.Name);

            command.Parameters.AddWithValue("@BinanceID", FuturesOrder.Id);
            command.Parameters.AddWithValue("@CreateTime", FuturesOrder.CreateTime);
            command.Parameters.AddWithValue("@OrderSide", FuturesOrder.Side.ToString());
            command.Parameters.AddWithValue("@OrderType", FuturesOrder.Type.ToString());
            command.Parameters.AddWithValue("@Price", FuturesOrder.Price);
            command.Parameters.AddWithValue("@Quantity", FuturesOrder.Quantity);

            command.Parameters.AddWithValue("@DateTime", Candlestick.Date);
            command.Parameters.AddWithValue("@Open", Candlestick.Open);
            command.Parameters.AddWithValue("@Close", Candlestick.Close);
            command.Parameters.AddWithValue("@High", Candlestick.High);
            command.Parameters.AddWithValue("@Low", Candlestick.Low);

            command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@Candlestick_Identity",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            });
            command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@FuturesOrder_Identity",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            });
            #endregion

            
            command.ExecuteNonQuery();
            this.Connection.Close();
            
            FuturesOrder_Id = (int)command.Parameters["@FuturesOrder_Identity"].Value;
            Candlestick_Id = (int)command.Parameters["@Candlestick_Identity"].Value;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
}
