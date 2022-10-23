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

            using SqlCommand command = new SqlCommand("spAddCandlestick", this.Connection);
            command.CommandType = CommandType.StoredProcedure;

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
                Direction = ParameterDirection.Output,
            }); 
            #endregion

            command.ExecuteNonQuery();
            
            this.Connection.Close();
            
            return (int)command.Parameters["@ScopeIdentity"].Value;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
    
    public int AddFuturesOrder(BinanceFuturesOrder futuresOrder)
    {
        try
        {
            this.Connection = this.ConnectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand("spAddFuturesOrder", this.Connection);
            command.CommandType = CommandType.StoredProcedure;

            #region SqlCommand parameters
            command.Parameters.AddWithValue("@Symbol", futuresOrder.Symbol);
            command.Parameters.AddWithValue("@BinanceID", futuresOrder.Id);
            command.Parameters.AddWithValue("@CreateTime", futuresOrder.CreateTime);
            command.Parameters.AddWithValue("@OrderSide", futuresOrder.Side.ToString());
            command.Parameters.AddWithValue("@OrderType", futuresOrder.Type.ToString());
            command.Parameters.AddWithValue("@Price", futuresOrder.Price);
            command.Parameters.AddWithValue("@Quantity", futuresOrder.Quantity);
            command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@ScopeIdentity",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            }); 
            #endregion

            command.ExecuteNonQuery();

            this.Connection.Close();
            
            return (int)command.Parameters["@ScopeIdentity"].Value;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
    public void AddFuturesOrder(BinanceFuturesOrder futuresOrder, int Candlestick_Identity, out int FuturesOrder_Identity)
    {
        try
        {
            this.Connection = this.ConnectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand("spAddFuturesOrder", this.Connection);
            command.CommandType = CommandType.StoredProcedure;

            #region SqlCommand parameters
            command.Parameters.AddWithValue("@CandlestickID", Candlestick_Identity);
            command.Parameters.AddWithValue("@Symbol", futuresOrder.Symbol);
            command.Parameters.AddWithValue("@BinanceID", futuresOrder.Id);
            command.Parameters.AddWithValue("@CreateTime", futuresOrder.CreateTime);
            command.Parameters.AddWithValue("@OrderSide", futuresOrder.Side.ToString());
            command.Parameters.AddWithValue("@OrderType", futuresOrder.Type.ToString());
            command.Parameters.AddWithValue("@Price", futuresOrder.Price);
            command.Parameters.AddWithValue("@Quantity", futuresOrder.Quantity);
            command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@ScopeIdentity",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output,
            });
            #endregion
            
            command.ExecuteNonQuery();
            
            this.Connection.Close();
            
            FuturesOrder_Identity = (int)command.Parameters["@ScopeIdentity"].Value;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
    public void AddFuturesOrder(BinanceFuturesOrder futuresOrder, TVCandlestick candlestick, out int FuturesOrder_Identity, out int Candlestick_Identity)
    {
        try
        {
            this.Connection = this.ConnectionFactory.CreateConnection();
            
            using SqlCommand command = new SqlCommand("spAddFuturesOrderAndCandlestick", this.Connection);
            command.CommandType = CommandType.StoredProcedure;

            #region SqlCommand parameters
            command.Parameters.AddWithValue("@CurrencyPair", candlestick.CurrencyPair.Name);

            command.Parameters.AddWithValue("@BinanceID", futuresOrder.Id);
            command.Parameters.AddWithValue("@CreateTime", futuresOrder.CreateTime);
            command.Parameters.AddWithValue("@OrderSide", futuresOrder.Side.ToString());
            command.Parameters.AddWithValue("@OrderType", futuresOrder.Type.ToString());
            command.Parameters.AddWithValue("@Price", futuresOrder.Price);
            command.Parameters.AddWithValue("@Quantity", futuresOrder.Quantity);

            command.Parameters.AddWithValue("@DateTime", candlestick.Date);
            command.Parameters.AddWithValue("@Open", candlestick.Open);
            command.Parameters.AddWithValue("@Close", candlestick.Close);
            command.Parameters.AddWithValue("@High", candlestick.High);
            command.Parameters.AddWithValue("@Low", candlestick.Low);

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
            
            FuturesOrder_Identity = (int)command.Parameters["@FuturesOrder_Identity"].Value;
            Candlestick_Identity = (int)command.Parameters["@Candlestick_Identity"].Value;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
}
