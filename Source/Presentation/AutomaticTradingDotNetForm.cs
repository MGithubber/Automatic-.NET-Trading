using System;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Serialization;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Application.Models;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Services;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

using Microsoft.Extensions.DependencyInjection;

using OpenQA.Selenium;

using Skender.Stock.Indicators;

namespace Presentation;

public partial class AutomaticTradingDotNetForm : Form
{
    public AutomaticTradingDotNetForm() => this.InitializeComponent();

    private void AutomaticTradingDotNetForm_Load(object sender, EventArgs e) { }


    //// //// //// //// //// //// //// ////
    
    private IPoolTradingService? MPoolTradingService;
    private Task? PoolTradingTask;
    private Task? ShowActiveBotsTask;
    
    private async Task StartPoolTradingAsync()
    {
        this.MPoolTradingService = TradingApplication.GetDefaultPoolTradingService();
        await this.MPoolTradingService!.StartTradingAsync();
    }
    private void StartButton_Click(object sender, EventArgs e)
    {
        this.PoolTradingTask ??= this.StartPoolTradingAsync();
        this.ShowActiveBotsTask ??= Task.Run(() =>
        {
            while (this.MPoolTradingService is null) continue;
            this.NrActiveBotsTextBox.BeginInvoke(() => this.NrActiveBotsTextBox.Text = this.MPoolTradingService.NrTradingStrategies.ToString());
        }); 
    }
    

    //// //// //// //// ////


    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        this.MPoolTradingService?.Dispose();
        base.OnFormClosed(e);
    }
}
