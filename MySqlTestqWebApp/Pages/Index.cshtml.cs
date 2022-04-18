using Microsoft.AspNetCore.Mvc.RazorPages;
using MOBZystems.Data;
using MySql.Data.MySqlClient;

namespace MySqlTestqWebApp.Pages
{
  public class IndexModel : PageModel
  {
    private readonly ILogger<IndexModel> _logger;
    public string? Result;

    public IndexModel(ILogger<IndexModel> logger)
    {
      _logger = logger;
    }

    public async Task OnGet()
    {
      var sb = new System.Text.StringBuilder();
      var database = "testdb1";

      try
      {
        // See https://aka.ms/new-console-template for more information
        var connectionString = Environment.GetEnvironmentVariable("DBCONN")!;
        sb.AppendLine($"Connection string: {connectionString}");
        using (var connection = new MySqlConnection(connectionString))
        {
          using (var dataConnection = new DataConnection(connection))
          {
            sb.AppendLine($"Connection open.");

            // Test if database exists
            var dbCount = await dataConnection.ExecuteScalarAsync<long>($"SELECT COUNT(SCHEMA_NAME) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{database}'");
            if (dbCount == 0)
            {
              sb.AppendLine("Database does not exist. Creating database...");
              await dataConnection.ExecuteNonQueryAsync($"CREATE SCHEMA IF NOT EXISTS `{database}` DEFAULT CHARACTER SET utf8");
              sb.AppendLine("Creating table...");
              await dataConnection.ExecuteNonQueryAsync(@$"
CREATE TABLE IF NOT EXISTS `{database}`.`item` (
  `itemid` INT NOT NULL AUTO_INCREMENT,
  `name` NVARCHAR(200) NOT NULL,
  PRIMARY KEY (`itemid`),
  UNIQUE INDEX `name_UNIQUE` (`name` ASC) VISIBLE)
ENGINE = InnoDB
");
              sb.AppendLine("Inserting rows...");
              await dataConnection.ExecuteNonQueryAsync("INSERT INTO `{database}`.`item` (`name`) VALUES ('markus'), ('tobias');");
              sb.AppendLine("Database created.");
            }
            else
            {
              sb.AppendLine("Database exists.");
            }

            // Insert a dummy record. Paramters specified as name, value; order unimportant.
            // Parameters are named
            // await dataConnection.ExecuteNonQueryAsync("Insert into item (itemid, `name`) values (@id, @name)", "name", "test123", "id", 3);

            var result = await dataConnection.SelectAsync("select * from item");
            foreach (var row in result)
            {
              sb.AppendLine($"{row["itemid"]}: {row["name"]}");
            }
            sb.AppendLine($"{result.Count()} items found.");
          }
        }
        sb.AppendLine("Done.");

      }
      //catch
      //{
      //}
      finally
      {
        this.Result = sb.ToString();
      }
    }
  }
}