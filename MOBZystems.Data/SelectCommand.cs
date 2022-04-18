using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace MOBZystems.Data
{
  /// <summary>
  /// A generic wrapper around a DbCommand with easier syntax for parameters
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
    ///// <summary>
    ///// Holds a parameter and associated value
    ///// </summary>
    //public class Parameter
    //{
    //  public string Name;
    //  public object Value;
    //}

    protected DbCommand _command = null;

    /// <summary>
    /// Constructor without parameters. Use AddParameters() to add parameters later
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="commandText"></param>
    /// <param name="commandType"></param>
    public DataCommand(string commandText, DbConnection connection, CommandType commandType = CommandType.Text)
    {
      _command = connection.CreateCommand();
      _command.CommandText = commandText;
      _command.CommandType = commandType;
    }

    ///// <summary>
    ///// Add the specified parameters
    ///// </summary>
    ///// <param name="parameters"></param>
    //protected void AddParameters(params Parameter[] parameters)
    //{
    //  foreach (var p in parameters)
    //  {
    //    var param = _command.CreateParameter();
    //    param.ParameterName = p.Name;
    //    param.Value = p.Value;
    //    _command.Parameters.Add(param);
    //  }
    //}

    ///// <summary>
    ///// Create a new SelectCommand with the specified command type and parameters
    ///// </summary>
    ///// <param name="connection"></param>
    ///// <param name="commandText"></param>
    ///// <param name="commandType"></param>
    ///// <param name="parameters"></param>
    //public SelectCommand(string commandText, DbConnection connection, CommandType commandType, params Parameter[] parameters) : this(commandText, connection, commandType)
    //{
    //  AddParameters(parameters);
    //}

    ///// <summary>
    ///// Create a new SelectCommand with the specified parameters
    ///// </summary>
    ///// <param name="connection"></param>
    ///// <param name="commandText"></param>
    ///// <param name="parameters"></param>
    //public SelectCommand(string commandText, DbConnection connection, params Parameter[] parameters) : this(commandText, connection, CommandType.Text, parameters)
    //{
    //}

    /// <summary>
    /// Add parameters, each specified as a name/value pair:
    /// 
    /// name1, value1, name2, value2, ...
    /// </summary>
    /// <param name="parameters"></param>
    public void AddParameters(params object[] parameters)
    {
      if ((parameters.Length % 2) != 0)
      {
        throw new ArgumentException($"{nameof(parameters)} must contain (name, value) pairs and cannot have odd length");
      }

      for (int i = 0; i < parameters.Length; i += 2)
      {
        var name = parameters[i] as string;
        if (name == null)
          throw new ArgumentException($"{nameof(parameters)}[{i}] must have type string");
        var param = _command.CreateParameter();
        param.ParameterName = name;
        param.Value = parameters[i + 1];
        _command.Parameters.Add(param);
      }
    }

    /// <summary>
    /// Create a new SelectCommand with the specified command type and parameters
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="commandText"></param>
    /// <param name="commandType"></param>
    /// <param name="parameters"></param>
    public DataCommand(string commandText, DbConnection connection, CommandType commandType, params object[] parameters) : this(commandText, connection, commandType)
    {
      AddParameters(parameters);
    }

    /// <summary>
    /// Create a new SelectCommand with the specified parameters
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="commandText"></param>
    /// <param name="parameters"></param>
    public DataCommand(string commandText, DbConnection connection, params object[] parameters) : this(commandText, connection, CommandType.Text, parameters)
    {
    }

    public async Task<int> ExecuteNonQueryAsync()
    {
      return await _command.ExecuteNonQueryAsync();
    }

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
  /// Use SelectAsync to get a SelectResult
  /// </summary>
  public class SelectCommand: DataCommand
  {
    public SelectCommand(string commandText, DbConnection connection, CommandType commandType = CommandType.Text) :
      base(commandText, connection, commandType)
    {
    }

    public SelectCommand(string commandText, DbConnection connection, CommandType commandType, params object[] parameters) : 
      base(commandText, connection, commandType, parameters)
    {
    }

    public SelectCommand(string commandText, DbConnection connection, params object[] parameters) :
      this(commandText, connection, CommandType.Text, parameters)
    {
    }

    /// <summary>
    /// Read all rows returned by the this command
    /// </summary>
    /// <returns>A SelectResult object, containing column information and row data</returns>
    public async Task<SelectResult> ResultAsync()
    {
      var result = new SelectResult();
      using (var reader = await _command.ExecuteReaderAsync())
      {
        await result.ReadAsync(reader);
      }
      return result;
    }

    public async Task<T> ExecuteScalarAsync<T>()
    {
      return (T)await _command.ExecuteScalarAsync();
    }
  }
}
