using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace MOBZystems.Data
{
  /// <summary>
  /// A generic wrapper around a DbCommand with easier syntax for parameters
  /// 
  /// Supports ExecuteNonQuery()
  ///
  /// Usage:
  /// 
  /// using (var s = new DataCommand(
  ///  connection,
  ///  "Select * From ... Where A=@A and B=@B",
  ///  "A", [value-for-A],
  ///  "B", [value-for-B],
  ///  [param-name], [param-value], ...
  /// ) {
  /// }
  /// </summary>
  public class DataCommand : IDisposable
  {
    protected DbCommand _command = null;

    /// <summary>
    /// Constructor without parameters. Use AddParameters() to add parameters later
    /// </summary>
    /// <param name="connection">The connection to create the command for</param>
    /// <param name="commandText">The SQL text of the cmmand</param>
    /// <param name="commandType">The type of command</param>
    public DataCommand(string commandText, DbConnection connection, CommandType commandType = CommandType.Text)
    {
      if (connection == null)
        throw new ArgumentNullException(nameof(connection));

      _command = connection.CreateCommand();
      _command.CommandText = commandText;
      _command.CommandType = commandType;
    }

    /// <summary>
    /// Add parameters, each specified as a tuple of name/value:
    /// 
    /// (name1, value1), (name2, value2), ...
    /// </summary>
    /// <param name="parameters"></param>
    public void AddParameters(params (string name, object value)[] parameters)
    {
      foreach (var p in parameters)
      {
        if (p.name == null)
          throw new ArgumentNullException(nameof(p), "Name of parameter cannot be null");

        var param = _command.CreateParameter();
        param.ParameterName = p.name;
        param.Value = p.value;
        _command.Parameters.Add(param);
      }
    }

    /// <summary>
    /// Create a new SelectCommand with the specified command type and parameters
    /// </summary>
    public DataCommand(string commandText, DbConnection connection, CommandType commandType, params (string name, object value)[] parameters) : 
      this(commandText, connection, commandType)
    {
      AddParameters(parameters);
    }

    /// <summary>
    /// Create a new SelectCommand with the specified parameters
    /// </summary>
    public DataCommand(string commandText, DbConnection connection, params (string name, object value)[] parameters) : 
      this(commandText, connection, CommandType.Text, parameters) {}

    /// <summary>
    /// Execute the command and return the number of rows affected
    /// </summary>
    public async Task<int> ExecuteNonQueryAsync() => await _command.ExecuteNonQueryAsync();

    #region IDisposable Support
    private bool _disposed = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          if (_command != null)
          {
            _command.Dispose();
            _command = null;
          }
        }

        _disposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }

  /// <summary>
  /// A generic wrapper around a DbCommand specific for select queries
  /// 
  /// Supports ExecuteScalar(), but not ExecuteReader. For that, use <see cref="ResultAsync"/> to get a <see cref="SelectResult"/>
  /// </summary>
  public class SelectCommand: DataCommand
  {
    public SelectCommand(string commandText, DbConnection connection, CommandType commandType = CommandType.Text) :
      base(commandText, connection, commandType) {}

    public SelectCommand(string commandText, DbConnection connection, CommandType commandType, params (string name, object value)[] parameters) : 
      base(commandText, connection, commandType, parameters) {}

    public SelectCommand(string commandText, DbConnection connection, params (string name, object value)[] parameters) :
      this(commandText, connection, CommandType.Text, parameters) {}

    /// <summary>
    /// Read all rows returned by the this command
    /// </summary>
    /// <returns>A SelectResult object, containing column information and row data</returns>
    /// <remarks>It's probably more convenient to use <see cref="DataConnection.SelectAsync(string, CommandType, (string name, object value)[])" /></remarks>
    public async Task<SelectResult> ResultAsync()
    {
      var result = new SelectResult();
      using (var reader = await _command.ExecuteReaderAsync())
        await result.ReadAsync(reader);
      return result;
    }

    /// <summary>
    /// Execute the select command and return a scalar ot type T
    /// </summary>
    public async Task<T> ExecuteScalarAsync<T>() => (T)await _command.ExecuteScalarAsync();
  }
}
