using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using CryptoExchange.Net.CommonObjects;

namespace AutomaticDotNETtrading.Domain.Models;

[DebuggerDisplay("{Base, nq}{\"/\", nq}{Quote, nq}")]
public class CurrencyPair : ICloneable
{
    public string Base { get; }
    public string Quote { get; }
    
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string Name => $"{this.Base}{this.Quote}";
    
    public CurrencyPair(string Base, string Quote)
    {
        this.Base = Base ?? throw new ArgumentNullException(nameof(Base));
        this.Quote = Quote ?? throw new ArgumentNullException(nameof(Quote));
    }

    ////
    
    public object Clone() => this.MemberwiseClone();
    
    #region overrides
    public override string ToString() => this.Name;

    public override bool Equals(object? obj) => obj is CurrencyPair pair && this.Name == pair.Name;
    public override int GetHashCode() => HashCode.Combine(this.Base, this.Quote);
    #endregion
    
    #region operator overloading
    public static bool operator ==(CurrencyPair pair1, CurrencyPair pair2) => pair1.Base == pair2.Base && pair1.Quote == pair2.Quote;
    public static bool operator !=(CurrencyPair pair1, CurrencyPair pair2) => pair1.Base != pair2.Base || pair1.Quote != pair2.Quote;
    
    public static bool operator ==(CurrencyPair pair, Symbol symbol) => pair.Name == symbol.Name;
    public static bool operator !=(CurrencyPair pair, Symbol symbol) => pair.Name != symbol.Name;
    public static bool operator ==(Symbol symbol, CurrencyPair pair) => pair.Name == symbol.Name;
    public static bool operator !=(Symbol symbol, CurrencyPair pair) => pair.Name != symbol.Name;

    public static bool operator ==(CurrencyPair pair, string symbol) => pair.Name == symbol;
    public static bool operator !=(CurrencyPair pair, string symbol) => pair.Name != symbol;
    public static bool operator ==(string symbol, CurrencyPair pair) => pair.Name == symbol;
    public static bool operator !=(string symbol, CurrencyPair pair) => pair.Name != symbol;
    #endregion
}
