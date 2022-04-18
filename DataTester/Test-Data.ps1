Add-Type -Path .\bin\Debug\net6.0\MySql.Data.dll
Add-Type -Path .\bin\Debug\net6.0\MOBZystems.Data.dll

# Create a MySql connection (do not open yet!)
$c = [MySql.Data.MySqlClient.MySqlConnection]::new('Server=127.0.0.1;Database=testdb1;Uid=root;Pwd=laurier46;')
$dc = [MOBZystems.Data.DataConnection]::new($c)
try {
  $result = $dc.SelectAsync("select * from item").GetAwaiter().GetResult()
  foreach ($row in $result) {
    $row["itemid"].ToString() + ": " + $row["name"]
  }
}

finally {
  $dc.Dispose()
}
