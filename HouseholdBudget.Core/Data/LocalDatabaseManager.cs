using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace HouseholdBudget.Core.Data
{
    public class LocalDatabaseManager : IDatabaseManager
    {
        private string _dbFile;
        private readonly IAppConfiguration _appConfiguration;

        public LocalDatabaseManager(IAppConfiguration appConfiguration)
        {
            _dbFile = appConfiguration.GetValue("DatabaseFile");

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Transactions (
                Id TEXT PRIMARY KEY,
                UserId TEXT NOT NULL,
                Date TEXT NOT NULL,
                Description TEXT,
                Amount REAL NOT NULL,
                Currency TEXT NOT NULL,
                CategoryId TEXT NOT NULL,
                IsRecurring INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Categories (
                Id TEXT PRIMARY KEY,
                UserId TEXT NOT NULL,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL -- np. 0 = wydatek, 1 = przychód
            );
            ";
            command.ExecuteNonQuery();
        }

        public void SaveTransaction(Transaction transaction)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            INSERT INTO Transactions (Id, UserId, Date, Description, Amount, Currency, CategoryId, IsRecurring)
            VALUES ($id, $userId, $date, $desc, $amount, $currency, $catId, $recurring);
            ";
            command.Parameters.AddWithValue("$id", transaction.Id.ToString());
            command.Parameters.AddWithValue("$userId", transaction.UserId.ToString());
            command.Parameters.AddWithValue("$date", transaction.Date.ToString("o"));
            command.Parameters.AddWithValue("$desc", transaction.Description ?? "");
            command.Parameters.AddWithValue("$amount", transaction.Amount);
            command.Parameters.AddWithValue("$currency", JsonSerializer.Serialize(transaction.Currency));
            command.Parameters.AddWithValue("$catId", transaction.CategoryId.ToString());
            command.Parameters.AddWithValue("$recurring", transaction.IsRecurring ? 1 : 0);

            command.ExecuteNonQuery();
        }

        public List<Transaction> LoadTransactionsForUser(Guid userId)
        {
            var result = new List<Transaction>();

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Transactions WHERE UserId = $userId";
            command.Parameters.AddWithValue("$userId", userId.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Transaction
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    UserId = Guid.Parse(reader.GetString(1)),
                    Date = DateTime.Parse(reader.GetString(2)),
                    Description = reader.GetString(3),
                    Amount = reader.GetDecimal(4),
                    Currency = JsonSerializer.Deserialize<Currency>(reader.GetString(5)) ?? new Currency(),
                    CategoryId = Guid.Parse(reader.GetString(6)),
                    IsRecurring = reader.GetInt32(7) == 1
                });
            }

            return result;
        }

        public void DeleteTransaction(Guid transactionId)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Transactions WHERE Id = $id";
            command.Parameters.AddWithValue("$id", transactionId.ToString());

            command.ExecuteNonQuery();
        }

        public void UpdateTransaction(Transaction transaction)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            UPDATE Transactions
            SET Date = $date,
                Description = $desc,
                Amount = $amount,
                Currency = $currency,
                CategoryId = $catId,
                IsRecurring = $recurring
            WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$id", transaction.Id.ToString());
            command.Parameters.AddWithValue("$date", transaction.Date.ToString("o"));
            command.Parameters.AddWithValue("$desc", transaction.Description ?? "");
            command.Parameters.AddWithValue("$amount", transaction.Amount);
            command.Parameters.AddWithValue("$currency", JsonSerializer.Serialize(transaction.Currency));
            command.Parameters.AddWithValue("$catId", transaction.CategoryId.ToString());
            command.Parameters.AddWithValue("$recurring", transaction.IsRecurring ? 1 : 0);

            command.ExecuteNonQuery();
        }

        public void SaveCategory(Category category)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            INSERT OR REPLACE INTO Categories (Id, UserId, Name, Type)
            VALUES ($id, $userId, $name, $type);
            ";
            command.Parameters.AddWithValue("$id", category.Id.ToString());
            command.Parameters.AddWithValue("$userId", category.UserId.ToString());
            command.Parameters.AddWithValue("$name", category.Name);
            command.Parameters.AddWithValue("$type", (int)category.Type);
            command.ExecuteNonQuery();
        }

        public List<Category> LoadCategoriesForUser(Guid userId)
        {
            var result = new List<Category>();

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Categories WHERE UserId = $userId";
            command.Parameters.AddWithValue("$userId", userId.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Category
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    UserId = Guid.Parse(reader.GetString(1)),
                    Name = reader.GetString(2),
                    Type = (CategoryType)reader.GetInt32(3)
                });
            }

            return result;
        }

        public void DeleteCategory(Guid categoryId)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Categories WHERE Id = $id";
            command.Parameters.AddWithValue("$id", categoryId.ToString());
            command.ExecuteNonQuery();
        }
    }
}
