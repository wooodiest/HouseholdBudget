using HouseholdBudget.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction = HouseholdBudget.Models.Transaction;

namespace HouseholdBudget.Data
{
    public class DatabaseManager : IDatabaseManager
    {
        private string _dbFile;

        public DatabaseManager(string dbFile)
        {
            _dbFile = dbFile;

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Transactions (
                Id TEXT PRIMARY KEY,
                Date TEXT NOT NULL,
                Description TEXT,
                Amount REAL NOT NULL,
                CategoryId TEXT NOT NULL,
                IsRecurring INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Categories (
                Id TEXT PRIMARY KEY,
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
            INSERT INTO Transactions (Id, Date, Description, Amount, CategoryId, IsRecurring)
            VALUES ($id, $date, $desc, $amount, $catId, $recurring);
            ";
            command.Parameters.AddWithValue("$id",        transaction.Id.ToString());
            command.Parameters.AddWithValue("$date",      transaction.Date.ToString("o"));
            command.Parameters.AddWithValue("$desc",      transaction.Description ?? "");
            command.Parameters.AddWithValue("$amount",    transaction.Amount);
            command.Parameters.AddWithValue("$catId",     transaction.CategoryId.ToString());
            command.Parameters.AddWithValue("$recurring", transaction.IsRecurring ? 1 : 0);

            command.ExecuteNonQuery();
        }

        public List<Transaction> LoadTransactions()
        {
            var result = new List<Transaction>();

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Transactions";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var transaction = new Transaction
                {
                    Id          = Guid.Parse(reader.GetString(0)),
                    Date        = DateTime.Parse(reader.GetString(1)),
                    Description = reader.GetString(2),
                    Amount      = reader.GetDecimal(3),
                    CategoryId  = Guid.Parse(reader.GetString(4)),
                    IsRecurring = reader.GetInt32(5) == 1
                };

                result.Add(transaction);
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
                CategoryId = $catId,
                IsRecurring = $recurring
            WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$id",        transaction.Id.ToString());
            command.Parameters.AddWithValue("$date",      transaction.Date.ToString("o"));
            command.Parameters.AddWithValue("$desc",      transaction.Description ?? "");
            command.Parameters.AddWithValue("$amount",    transaction.Amount);
            command.Parameters.AddWithValue("$catId",     transaction.CategoryId.ToString());
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
            INSERT OR REPLACE INTO Categories (Id, Name, Type)
            VALUES ($id, $name, $type);
            ";
            command.Parameters.AddWithValue("$id",   category.Id.ToString());
            command.Parameters.AddWithValue("$name", category.Name);
            command.Parameters.AddWithValue("$type", (int)category.Type);
            command.ExecuteNonQuery();
        }

        public List<Category> LoadCategories()
        {
            var result = new List<Category>();

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Categories";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Category
                {
                    Id   = Guid.Parse(reader.GetString(0)),
                    Name = reader.GetString(1),
                    Type = (CategoryType)reader.GetInt32(2)
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
