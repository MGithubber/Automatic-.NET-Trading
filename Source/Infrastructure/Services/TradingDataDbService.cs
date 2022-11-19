using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlConnection = System.Data.SqlClient.SqlConnection;

using Binance.Net.Objects.Models.Futures;

using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Enums;

namespace AutomaticDotNETtrading.Infrastructure.Services;

public class TradingDataDbService : ITradingDataDbService<TVCandlestick>
{
    private readonly SqlDatabaseConnectionFactory ConnectionFactory;
    private SqlConnection? Connection;

    public TradingDataDbService(string ConnectionString) => this.ConnectionFactory = new SqlDatabaseConnectionFactory(ConnectionString);
    public TradingDataDbService(SqlDatabaseConnectionFactory connectionFactory) => this.ConnectionFactory = connectionFactory;

    //// //// ////

    public int AddCandlestick(TVCandlestick Candlestick)
    {
        try
        {
            this.Connection = this.ConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand
            {
                CommandText = "spAddCandlestick",
                Connection = this.Connection,
                CommandType = CommandType.StoredProcedure,
            };

            #region SqlCommand parameters
            command.Parameters.AddWithValue("@CurrencyPair", Candlestick.CurrencyPair.Name);
            command.Parameters.AddWithValue("@DateTime", Candlestick.Date);
            command.Parameters.AddWithValue("@Open", Candlestick.Open);
            command.Parameters.AddWithValue("@Close", Candlestick.Close);
            command.Parameters.AddWithValue("@High", Candlestick.High);
            command.Parameters.AddWithValue("@Low", Candlestick.Low);
            command.Parameters.AddWithValue("@LuxAlgoSignal", Candlestick.LuxAlgoSignal.ToString());
            command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@ScopeIdentity",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.ReturnValue,
            });
            #endregion


            command.ExecuteNonQuery();
            this.Connection.Close();

            int DbIdentity = (int)command.Parameters["@ScopeIdentity"].Value;
            if (DbIdentity < 1)
            {
                throw new ArgumentException($"A candlestick with {nameof(CurrencyPair)} == \"{Candlestick.CurrencyPair}\" and {nameof(Candlestick.Date)} == \"{Candlestick.Date}\" is already in the database", nameof(Candlestick));
            }

            return DbIdentity;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
    public int DeleteCandlestick(TVCandlestick Candlestick)
    {
        try
        {
            this.Connection = this.ConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand
            {
                CommandText = "spDeleteCandlestick",
                Connection = this.Connection,
                CommandType = CommandType.StoredProcedure,
            };

            #region SqlCommand parameters
            command.Parameters.AddWithValue("@CurrencyPair", Candlestick.CurrencyPair.Name);
            command.Parameters.AddWithValue("@DateTime", Candlestick.Date);
            command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@DeletedIdentity",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.ReturnValue,
            });
            #endregion


            command.ExecuteNonQuery();
            this.Connection.Close();

            int DeletedIdentity = (int)command.Parameters["@DeletedIdentity"].Value;
            if (DeletedIdentity < 1)
            {
                throw new ArgumentException($"A candlestick with {nameof(CurrencyPair)} == \"{Candlestick.CurrencyPair}\" and {nameof(Candlestick.Date)} == \"{Candlestick.Date}\" could not be deleted from the database", nameof(Candlestick));
            }

            return DeletedIdentity;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }

    public void AddFuturesOrder(BinanceFuturesOrder FuturesOrder, TVCandlestick Candlestick, out int FuturesOrder_Id, out int Candlestick_Id)
    {
        try
        {
            if (FuturesOrder.Symbol != Candlestick.CurrencyPair.Name)
            {
                throw new ArgumentException($"The currency pair of the {nameof(Candlestick)} ({Candlestick.CurrencyPair}) doesn't match the symbol of the {nameof(FuturesOrder)} ({FuturesOrder.Symbol})");
            }


            this.Connection = this.ConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand
            {
                CommandText = "spAddFuturesOrder",
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
            command.Parameters.AddWithValue("@LuxAlgoSignal", Candlestick.LuxAlgoSignal != LuxAlgoSignal.Hold ? Candlestick.LuxAlgoSignal.ToString() : LuxAlgoSignal.Hold.ToString());

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


            int FuturesOrder_Id_temp = (int)command.Parameters["@FuturesOrder_Identity"].Value;
            int Candlestick_Id_temp = (int)command.Parameters["@Candlestick_Identity"].Value;

            if (FuturesOrder_Id_temp < 1 || Candlestick_Id_temp < 1)
            {
                throw new ArgumentException("The specified futures order and/or candlestick could not be added to the database");
            }

            FuturesOrder_Id = FuturesOrder_Id_temp;
            Candlestick_Id = Candlestick_Id_temp;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
    public int DeleteFuturesOrder(BinanceFuturesOrder FuturesOrder)
    {
        try
        {
            this.Connection = this.ConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand
            {
                CommandText = "spDeleteFuturesOrder",
                Connection = this.Connection,
                CommandType = CommandType.StoredProcedure,
            };

            #region SqlCommand parameters
            command.Parameters.AddWithValue("@BinanceID", FuturesOrder.Id);
            command.Parameters.Add(new SqlParameter
            {
                ParameterName = "@DeletedIdentity",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.ReturnValue,
            });
            #endregion


            command.ExecuteNonQuery();
            this.Connection.Close();

            int DeletedIdentity = (int)command.Parameters["@DeletedIdentity"].Value;
            if (DeletedIdentity < 1)
            {
                throw new ArgumentException($"A binance futures order with {nameof(FuturesOrder.Id)} == {FuturesOrder.Id} could not be deleted from the database", nameof(FuturesOrder));
            }

            return (int)command.Parameters["@DeletedIdentity"].Value;
        }
        catch { throw; }
        finally { this.Connection?.Close(); }
    }
}
