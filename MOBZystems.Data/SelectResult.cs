using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using System.Collections;

namespace MOBZystems.Data
{
    /// <summary>
    /// The result of a SelectCommand
    /// 
    /// Also an IEnumerable of ResultRow
    /// </summary>
    public class SelectResult: IEnumerable<SelectResult.ResultRow>
  {
    /// <summary>
    /// Information about a result column
    /// </summary>
    public class ResultColumn
    {
      public int Index { get; }
      public string Name { get; }
      public Type DataType { get; }

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

      public object[] Values
      {
        get
        {
          return _data;
        }
      }

      private ResultRow() { }

      internal ResultRow(SelectResult selectResult, DbDataReader reader)
      {
        _selectResult = selectResult;
        _data = new object[reader.FieldCount];
        reader.GetValues(_data);
        // Get rid of DBNull-values:
        for (int i = 0; i < _data.Length; i++)
        {
          if (_data[i] is DBNull)
          {
            _data[i] = null;
          }
        }
      }

      /// <summary>
      /// Get the value with the specified column name
      /// </summary>
      /// <param name="columnName"></param>
      /// <returns>An object</returns>
      public object this[string columnName]
      {
        get
        {
          return _data[_selectResult.Column(columnName).Index];
        }
      }

      public object this[int index]
      {
        get
        {
          return _data[index];
        }
      }

      /// <summary>
      /// Get the value with the specified column name, cast to the specified type
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="columnName"></param>
      /// <returns>The value, converted to T</returns>
      public T Value<T>(string columnName)
      {
        return (T)this[columnName];
      }

      public T Value<T>(int index)
      {
        return (T)this[index];
      }

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
        {
          throw new ArgumentNullException(nameof(format));
        }
        T value = (T)_data[_selectResult.Column(columnName).Index];
        return string.Format($"{{0:{format}}}", value);
      }
    }

    /// <summary>
    /// Mapping from (case insensitive) column name to column index
    /// </summary>
    protected Dictionary<string, ResultColumn> _columns = new Dictionary<string, ResultColumn>(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// The row data in this SelectResult
    /// </summary>
    protected List<ResultRow> _rows = new List<ResultRow>();

    protected string[] _columnNames = null;

    /// <summary>
    /// Internal constructor, only used by SelectCommand before calling ReadSync()
    /// </summary>
    internal SelectResult()
    {
    }

    /// <summary>
    /// Read from a data reader
    /// </summary>
    /// <param name="reader"></param>
    public async Task ReadAsync(DbDataReader reader)
    {
      _columnNames = new string[reader.FieldCount];

      // The list of column names and types is available before reading
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
      {
        _rows.Add(new ResultRow(this, reader));
      }
    }

    public ResultColumn Column(string columnName)
    {
      ResultColumn columnInfo;
      if (_columns.TryGetValue(columnName, out columnInfo))
      {
        return columnInfo;
      }

      throw new KeyNotFoundException($"{this.GetType().Name} does not contain column '{columnName}'");
    }

    public string[] ColumnNames
    {
      get
      {
        return _columnNames;
      }
    }

    #region IEnumerable
    public IEnumerator<ResultRow> GetEnumerator()
    {
      return ((IEnumerable<ResultRow>)_rows).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<ResultRow>)_rows).GetEnumerator();
    }
    #endregion
  }
}
