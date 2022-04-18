using MOBZystems.Data;
using MySql.Data.MySqlClient;

var connectionString = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");
Console.WriteLine($"Connection string: {connectionString}");
using (var connection = new MySqlConnection(connectionString))
{
  // connection.Open(); -- no need to do this, the DataConnection will do it

  using (var dataConnection = new DataConnection(connection))
  {
    Console.WriteLine($"Connection open.");

    // Test if database exists
    var dbCount = await dataConnection.ExecuteScalarAsync<long>("SELECT COUNT(SCHEMA_NAME) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'testdb1'");
    if (dbCount == 0)
    {
      Console.WriteLine("Database does not exist. Creating database...");
      await dataConnection.ExecuteNonQueryAsync("CREATE SCHEMA IF NOT EXISTS `testdb1` DEFAULT CHARACTER SET utf8");
      Console.WriteLine("Creating table...");
      await dataConnection.ExecuteNonQueryAsync(@"
CREATE TABLE IF NOT EXISTS `testdb1`.`item` (
  `itemid` INT NOT NULL,
  `name` NVARCHAR(200) NOT NULL,
  PRIMARY KEY (`itemid`),
  UNIQUE INDEX `name_UNIQUE` (`name` ASC) VISIBLE)
ENGINE = InnoDB
");
      Console.WriteLine("Inserting rows...");
      await dataConnection.ExecuteNonQueryAsync("INSERT INTO `testdb1`.`item` (`itemid`, `name`) VALUES (1, 'markus'), (2, 'tobias');");
      Console.WriteLine("Database created.");
    }
    else
    {
      Console.WriteLine("Database exists.");
    }

    var result = await dataConnection.SelectAsync("select * from item where itemid > @minitemid order by itemid", ("minitemid", -1));
    foreach (var row in result)
    {
      Console.WriteLine($"{row["itemid"]}: {row["name"]}");
    }
    Console.WriteLine($"{result.Count()} items found.");
  }
}
Console.WriteLine("Done.");