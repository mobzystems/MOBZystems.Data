# MOBZystems.Data

A simple set of wrappers around IDbConnection, IDbCommand etc. with a "better" data reader.

## SelectResult

The standard System.Data.IDbxxx objects are quite easy to use, except IDbDataReader, which is really cumbersome to use.

The `SelectResult` aims to offer a simpler alternative. Usage is:

```csharp
using (var dc = new DataConnection(IDbConnection)) {
  
  var result = await dc.SelectAsync("select * from ...");
  foreach (var row in result) {
    var id = row.Value<int>("ID");
  }
}
```

So:

- Create a connection to your favorite database. As long as it's an IDbConnection!
- Create a new DataConnection with it. If you supply `open` as `true`, the DataConnection will `Open()` the connection
- Disposing of the DataConnection will `Dispose()` of the underlying IDbConnection
- Call `SelectAsync(query)` on the DataConnection to get a `SelectResult`
- Iterate over the rows of the result
- Call `.Value<type>` on a row to retrieve column values.

### Fast but large

The `SelectResult` will *read the entire result set into an array* for optimal performance and least hassle. This makes it unsuitable for huge result sets - use an old-fashioned DataReader for that.
