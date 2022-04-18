using System;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace MOBZystems.Data
{
  /// <summary>
  /// Thin wrapper around a generic DbConnection. Created from an existing DbConnection, which is disposed on Dispose().
  /// Usage:
  /// 
  /// using (var dc = new DataConnection(some-connection)) {
  ///   using (var c = dc.CreateSelectOrDataCommand(...)) {
  ///   }
  /// }
  /// 
  /// Alternatively, exeute queries directly on the connection using SelectAsync() or ExecuteXXXAsync()
  /// </summary>
  public class DataConnection : IDisposable
  {
    protected DbConnection _connection = null;

    public DbConnection Connection { get; }

    public DataConnection(DbConnection connection, bool open = true)
    {
      _connection = connection;
      if (open)
        _connection.Open();
    }

    public SelectCommand CreateSelectCommand(string commandText, CommandType commandType, params object[] parameters)
    {
      return new SelectCommand(commandText, _connection, commandType, parameters);
    }

    public SelectCommand CreateSelectCommand(string commandText, params object[] parameters)
    {
      return new SelectCommand(commandText, _connection, CommandType.Text, parameters);
    }

    public DataCommand CreateDataCommand(string commandText, CommandType commandType, params object[] parameters)
    {
      return new DataCommand(commandText, _connection, commandType, parameters);
    }

    public DataCommand CreateDataCommand(string commandText, params object[] parameters)
    {
      return new DataCommand(commandText, _connection, CommandType.Text, parameters);
    }

    public async Task<SelectResult> SelectAsync(string commandText, CommandType commandType, params object[] parameters)
    {
      using (var sc = CreateSelectCommand(commandText, commandType, parameters))
      {
        return await sc.ResultAsync();
      }
    }

    public async Task<SelectResult> SelectAsync(string commandText, params object[] parameters)
    {
      return await SelectAsync(commandText, CommandType.Text, parameters);
    }

    public async Task<T> ExecuteScalarAsync<T>(string commandText, CommandType commandType, params object[] parameters)
    {
      using (var c = CreateSelectCommand(commandText, commandType, parameters))
      {
        return await c.ExecuteScalarAsync<T>();
      }
    }

    public async Task<T> ExecuteScalarAsync<T>(string commandText, params object[] parameters)
    {
      return await ExecuteScalarAsync<T>(commandText, CommandType.Text, parameters);
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType, params object[] parameters)
    {
      using (var c = CreateDataCommand(commandText, commandType, parameters))
      {
        return await c.ExecuteNonQueryAsync();
      }
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, params object[] parameters)
    {
      return await ExecuteNonQueryAsync(commandText, CommandType.Text, parameters);
    }

    #region IDisposable Support
    /// <summary>
    /// Dispose of the underlying connection
    /// </summary>
    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposed)
      {
        if (disposing)
        {
          if (_connection != null)
          {
            _connection.Dispose();
            _connection = null;
          }
        }
        disposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}
