using System;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace MOBZystems.Data
{
  /// <summary>
  /// Thin wrapper around a generic DbConnection. Created from an existing DbConnection, which is disposed on Dispose().
  /// 
  /// Primarily used to provide convenience commands:
  /// 
  /// - ExecuteAsync/ExecuteScalarAsync/ExecuteNonQueryAsync
  /// - CreateSelectCommand
  /// - CreateDataCommand (TODO: no use yet)
  ///
  /// Usage:
  /// 
  /// using (var dc = new DataConnection(some-connection)) {
  ///   using (var c = dc.CreateSelectOrDataCommand(...)) {
  ///   }
  ///   var result = await dc.SelectAsync("select * from ...")
  /// }
  /// 
  /// TODO: let the constructor decide on the state of the connection
  /// 
  /// Most of the heavy lifting is done by <see cref="SelectCommand"/>.
  /// </summary>
  /// <remarks>The connection provided can be open on construction, but it is always closed when disposing!</remarks>
  public class DataConnection : IDisposable
  {
    // The actual DbConnection
    protected DbConnection _connection = null;
    public DbConnection Connection { get; }

    /// <summary>
    /// Create a new <see cref="DataConnection"/> on an existing <see cref="DbConnection"/>
    /// </summary>
    /// <param name="connection">The connection to use</param>
    /// <param name="open">If true (default) open the connection. If false, assume it open already</param>
    public DataConnection(DbConnection connection, bool open = true)
    {
      _connection = connection;
      if (open)
        _connection.Open();
    }

    #region "Create commands"
    public SelectCommand CreateSelectCommand(string commandText, CommandType commandType, params (string name, object value)[] parameters)
      => new SelectCommand(commandText, _connection, commandType, parameters);
    
    public SelectCommand CreateSelectCommand(string commandText, CommandType commandType, int timeout, params (string name, object value)[] parameters)
      => new SelectCommand(commandText, _connection, commandType, timeout, parameters);

    public SelectCommand CreateSelectCommand(string commandText, params (string name, object value)[] parameters)
      => new SelectCommand(commandText, _connection, CommandType.Text, parameters);

    public SelectCommand CreateSelectCommand(string commandText, int timeout, params (string name, object value)[] parameters)
      => new SelectCommand(commandText, _connection, CommandType.Text, timeout, parameters);

    public DataCommand CreateDataCommand(string commandText, CommandType commandType, params (string name, object value)[] parameters)
      => new DataCommand(commandText, _connection, commandType, parameters);

    public DataCommand CreateDataCommand(string commandText, CommandType commandType, int timeout, params (string name, object value)[] parameters)
      => new DataCommand(commandText, _connection, commandType, timeout, parameters);

    public DataCommand CreateDataCommand(string commandText, params (string name, object value)[] parameters)
      => new DataCommand(commandText, _connection, CommandType.Text, parameters);

    public DataCommand CreateDataCommand(string commandText, int timeout, params (string name, object value)[] parameters)
      => new DataCommand(commandText, _connection, CommandType.Text, timeout, parameters);

    public async Task<SelectResult> SelectAsync(string commandText, CommandType commandType, params (string name, object value)[] parameters)
    {
      using (var sc = CreateSelectCommand(commandText, commandType, parameters))
        return await sc.ResultAsync();
    }
    
    public async Task<SelectResult> SelectAsync(string commandText, CommandType commandType, int timeout, params (string name, object value)[] parameters)
    {
      using (var sc = CreateSelectCommand(commandText, commandType, timeout, parameters))
        return await sc.ResultAsync();
    }
    #endregion

    #region "Query execution"
    public async Task<SelectResult> SelectAsync(string commandText, params (string name, object value)[] parameters)
      => await SelectAsync(commandText, CommandType.Text, parameters);

    public async Task<SelectResult> SelectAsync(string commandText, int timeout, params (string name, object value)[] parameters)
      => await SelectAsync(commandText, CommandType.Text, timeout, parameters);
    
    public async Task<T> ExecuteScalarAsync<T>(string commandText, CommandType commandType, params (string name, object value)[] parameters)
    {
      using (var c = CreateSelectCommand(commandText, commandType, parameters))
        return await c.ExecuteScalarAsync<T>();
    }

    public async Task<T> ExecuteScalarAsync<T>(string commandText, CommandType commandType, int timeout, params (string name, object value)[] parameters)
    {
      using (var c = CreateSelectCommand(commandText, commandType, timeout, parameters))
        return await c.ExecuteScalarAsync<T>();
    }

    public async Task<T> ExecuteScalarAsync<T>(string commandText, params (string name, object value)[] parameters)
      => await ExecuteScalarAsync<T>(commandText, CommandType.Text, parameters);

    public async Task<T> ExecuteScalarAsync<T>(string commandText, int timeout, params (string name, object value)[] parameters)
      => await ExecuteScalarAsync<T>(commandText, CommandType.Text, timeout, parameters);

    public async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType, params (string name, object value)[] parameters)
    {
      using (var c = CreateDataCommand(commandText, commandType, parameters))
        return await c.ExecuteNonQueryAsync();
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType, int timeout, params (string name, object value)[] parameters)
    {
      using (var c = CreateDataCommand(commandText, commandType, timeout, parameters))
        return await c.ExecuteNonQueryAsync();
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, params (string name, object value)[] parameters)
      => await ExecuteNonQueryAsync(commandText, CommandType.Text, parameters);

    public async Task<int> ExecuteNonQueryAsync(string commandText, int timeout, params (string name, object value)[] parameters)
      => await ExecuteNonQueryAsync(commandText, CommandType.Text, timeout, parameters);
    #endregion

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
