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
                Type INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS BudgetPeriods (
                Id TEXT PRIMARY KEY,
                UserId TEXT NOT NULL,
                Name TEXT NOT NULL,
                StartDate TEXT,
                EndDate TEXT,
                BaseCurrency TEXT NOT NULL,
                Status INTEGER NOT NULL,
                Notes TEXT
            );
            
            CREATE TABLE IF NOT EXISTS Budgets (
                Id TEXT PRIMARY KEY,
                UserId TEXT NOT NULL,
                BudgetPeriodId TEXT NOT NULL,
                CategoryId TEXT NOT NULL,
                Name TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                BudgetLimit REAL NOT NULL,
                Used REAL NOT NULL,
                Currency TEXT NOT NULL
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
            INSERT OR REPLACE INTO Transactions (Id, UserId, Date, Description, Amount, Currency, CategoryId, IsRecurring)
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

        public List<Budget> LoadBudgetsForUser(Guid userId)
        {
            var result = new List<Budget>();

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Budgets WHERE UserId = $userId";
            command.Parameters.AddWithValue("$userId", userId.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Budget
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    UserId = Guid.Parse(reader.GetString(1)),
                    BudgetPeriodId = Guid.Parse(reader.GetString(2)),
                    CategoryId = Guid.Parse(reader.GetString(3)),
                    Name = reader.GetString(4),
                    CreatedAt = DateTime.Parse(reader.GetString(5)),
                    Limit = reader.GetDecimal(6),
                    Used = reader.GetDecimal(7),
                    Currency = JsonSerializer.Deserialize<Currency>(reader.GetString(8)) ?? new Currency()
                });
            }

            return result;
        }

        public void SaveBudget(Budget budget)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            INSERT OR REPLACE INTO Budgets (Id, UserId, BudgetPeriodId, CategoryId, Name, CreatedAt, BudgetLimit, Used, Currency)
            VALUES ($id, $userId, $periodId, $categoryId, $name, $createdAt, $budgetLimit, $used, $currency);
            ";
            command.Parameters.AddWithValue("$id", budget.Id.ToString());
            command.Parameters.AddWithValue("$userId", budget.UserId.ToString());
            command.Parameters.AddWithValue("$periodId", budget.BudgetPeriodId.ToString());
            command.Parameters.AddWithValue("$categoryId", budget.CategoryId.ToString());
            command.Parameters.AddWithValue("$name", budget.Name);
            command.Parameters.AddWithValue("$createdAt", budget.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("$budgetLimit", budget.Limit);
            command.Parameters.AddWithValue("$used", budget.Used);
            command.Parameters.AddWithValue("$currency", JsonSerializer.Serialize(budget.Currency));

            command.ExecuteNonQuery();
        }
        public void DeleteBudget(Guid budgetId)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Budgets WHERE Id = $id";
            command.Parameters.AddWithValue("$id", budgetId.ToString());

            command.ExecuteNonQuery();
        }

        public List<BudgetPeriod> LoadBudgetPeriodsForUser(Guid userId)
        {
            var result = new List<BudgetPeriod>();

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM BudgetPeriods WHERE UserId = $userId";
            command.Parameters.AddWithValue("$userId", userId.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new BudgetPeriod
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    UserId = Guid.Parse(reader.GetString(1)),
                    Name = reader.GetString(2),
                    StartDate = string.IsNullOrWhiteSpace(reader.GetString(3)) ? null : DateTime.Parse(reader.GetString(3)),
                    EndDate = string.IsNullOrWhiteSpace(reader.GetString(4)) ? null : DateTime.Parse(reader.GetString(4)),
                    BaseCurrency = JsonSerializer.Deserialize<Currency>(reader.GetString(5)) ?? new Currency(),
                    Status = (BudgetPeriodStatus)reader.GetInt32(6),
                    Notes = reader.GetString(7)
                });
            }

            return result;
        }

        public void SaveBudgetPeriod(BudgetPeriod period)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            INSERT OR REPLACE INTO BudgetPeriods (Id, UserId, Name, StartDate, EndDate, BaseCurrency, Status, Notes)
            VALUES ($id, $userId, $name, $startDate, $endDate, $currency, $status, $notes);
            ";
            command.Parameters.AddWithValue("$id", period.Id.ToString());
            command.Parameters.AddWithValue("$userId", period.UserId.ToString());
            command.Parameters.AddWithValue("$name", period.Name);
            command.Parameters.AddWithValue("$startDate", period.StartDate?.ToString("o"));
            command.Parameters.AddWithValue("$endDate", period.EndDate?.ToString("o"));
            command.Parameters.AddWithValue("$currency", JsonSerializer.Serialize(period.BaseCurrency));
            command.Parameters.AddWithValue("$status", (int)period.Status);
            command.Parameters.AddWithValue("$notes", period.Notes);

            command.ExecuteNonQuery();
        }
        public void DeleteBudgetPeriod(Guid budgetPeriodId)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM BudgetPeriods WHERE Id = $id";
            command.Parameters.AddWithValue("$id", budgetPeriodId.ToString());

            command.ExecuteNonQuery();
        }

    }
}
