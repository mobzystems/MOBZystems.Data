using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using System.Collections;

namespace MOBZystems.Data
{
  /// <summary>
  /// The result of <see cref="SelectCommand.ResultAsync"/> or <see cref="DataConnection.SelectAsync(string, (string name, object value)[])"/>
  /// 
  /// Also an IEnumerable of <see cref="ResultRow"/>
  /// </summary>
  public class SelectResult: IEnumerable<SelectResult.ResultRow>
  {
    /// <summary>
    /// Information about a result column
    /// </summary>
    public class ResultColumn
    {
      // The index of this column in the SelectResult
      public readonly int Index;
      // The name of this column
      public readonly string Name;
      // The type of this column
      public readonly Type DataType;

      internal ResultColumn(int index, string name, Type dataType)
      {
        Index = index;
        Name = name;
        DataType = dataType;
      }
    }

    /// <summary>
    /// Data in a result row
    /// </summary>
    public class ResultRow
    {
      // The SelectResult we're part of (for access to column information)
      protected SelectResult _selectResult;
      // The data in this row as object, in column order
      protected object[] _data;

      public object[] Values => _data;

      // Private default constructor to prevent creation by outsiders
      private ResultRow() { }

      /// <summary>
      /// Internal constructor fo use by SelectCommand
      /// </summary>
      /// <param name="selectResult">The <see cref="SelectResult"/> this row is a part of</param>
      /// <param name="reader">The <see cref="DbDataReader"/> to read data from</param>
      internal ResultRow(SelectResult selectResult, DbDataReader reader)
      {
        // Store the parent
        _selectResult = selectResult;
        // Allocate and read the data
        _data = new object[reader.FieldCount];
        reader.GetValues(_data);

        // Get rid of DBNull-values, replacing them with null:
        for (int i = 0; i < _data.Length; i++)
          if (_data[i] is DBNull)
            _data[i] = null;
      }

      /// <summary>
      /// Get the value with the specified column name as an object
      /// </summary>
      public object this[string columnName] => _data[_selectResult.Column(columnName).Index];

      /// <summary>
      /// Get the value of the column with the specified column index as an object
      /// </summary>
      public object this[int index] => _data[index];

      /// <summary>
      /// Get the value with the specified column name, cast to the specified type
      /// </summary>
      public T Value<T>(string columnName) => (T)this[columnName];

      /// <summary>
      /// Get the value of the column with the specified column index, cast to the specified type
      /// </summary>
      public T Value<T>(int index) => (T)this[index];

      /// <summary>
      /// Get the value of the specified column, formatted according to the specified format
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="columnName"></param>
      /// <param name="format"></param>
      /// <returns>A string</returns>
      public string Value<T>(string columnName, string format)
      {
        if (format == null)
          throw new ArgumentNullException(nameof(format));

        T value = Value<T>(columnName);
        return string.Format($"{{0:{format}}}", value);
      }
    }

    // The simple list of column names
    protected string[] _columnNames = null;
    public string[] ColumnNames => _columnNames;

    /// <summary>
    /// Mapping from (case insensitive) column name to <see cref="ResultColumn"/>
    /// </summary>
    protected Dictionary<string, ResultColumn> _columns = new Dictionary<string, ResultColumn>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The row data in this SelectResult
    /// </summary>
    protected List<ResultRow> _rows = new List<ResultRow>();

    /// <summary>
    /// Internal constructor, only used by SelectCommand before calling ReadSync()
    /// </summary>
    internal SelectResult() {}

    /// <summary>
    /// Read from a data reader
    /// </summary>
    /// <param name="reader"></param>
    public async Task ReadAsync(DbDataReader reader)
    {
      _columnNames = new string[reader.FieldCount];

      // The list of column names and types is available before reading
      // from the data reader
      for (int i = 0; i < reader.FieldCount; i++)
      {
        var name = reader.GetName(i);
        _columnNames[i] = name;
        _columns.Add(
          name,
          new ResultColumn(i, name, reader.GetFieldType(i))
        );
      }

      // Now iterate the rows of the result
      while (await reader.ReadAsync())
        _rows.Add(new ResultRow(this, reader));
    }

    /// <summary>
    /// Get the <see cref="ResultColumn"/> from the column name.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If the column name is nor part of the <see cref="ResultColumn"/></exception>
    public ResultColumn Column(string columnName)
    {
      if (_columns.TryGetValue(columnName, out ResultColumn columnInfo))
        return columnInfo;

      throw new KeyNotFoundException($"{this.GetType().Name} does not contain column '{columnName}'");
    }

    #region IEnumerable
    // Type safe enumerators
    public IEnumerator<ResultRow> GetEnumerator() => ((IEnumerable<ResultRow>)_rows).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ResultRow>)_rows).GetEnumerator();
    #endregion
  }
}
