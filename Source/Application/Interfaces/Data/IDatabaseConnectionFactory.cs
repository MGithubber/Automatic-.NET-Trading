using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticDotNETtrading.Application.Interfaces.Data;

public interface IDatabaseConnectionFactory<T> where T : IDbConnection
{
    public T CreateConnection();
}
