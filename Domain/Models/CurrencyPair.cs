﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticDotNETtrading.Domain.Models;

public class CurrencyPair : ICloneable
{
    public string Base { get; }
    public string Quote { get; }
    public string Name => $"{this.Base}{this.Quote}";
    
    public CurrencyPair(string Base, string Quote)
    {
        this.Base = Base ?? throw new ArgumentNullException(nameof(Base));
        this.Quote = Quote ?? throw new ArgumentNullException(nameof(Quote));
    }

    ////
    
    public object Clone() => this.MemberwiseClone();
    
    #region overrides
    public override string ToString() => $"{this.Base}{this.Quote}";

    public override bool Equals(object? obj) => obj is CurrencyPair pair && this.Name == pair.Name;
    public override int GetHashCode() => HashCode.Combine(this.Base, this.Quote);
    #endregion
    
    #region operator overloading
    public static bool operator ==(CurrencyPair pair1, CurrencyPair pair2) => pair1.Base == pair2.Base && pair1.Quote == pair2.Quote;
    public static bool operator !=(CurrencyPair pair1, CurrencyPair pair2) => pair1.Base != pair2.Base || pair1.Quote != pair2.Quote; 
    #endregion
}