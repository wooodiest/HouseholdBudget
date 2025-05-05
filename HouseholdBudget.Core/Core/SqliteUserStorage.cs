using HouseholdBudget.Core.Models;
using Microsoft.Data.Sqlite;

namespace HouseholdBudget.Core.Core
{
    public class SqliteUserStorage : IUserStorage
    {
        private readonly string _dbFile;
        private readonly IAppConfiguration _appConfiguration;

        public SqliteUserStorage(IAppConfiguration appConfiguration)
        {
            _dbFile = appConfiguration.GetValue("UserStorageFile");
            EnsureUserTableExists();
        }

        private void EnsureUserTableExists()
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                DeafaultCurrencyCode TEXT NOT NULL
            );
            ";
            command.ExecuteNonQuery();
        }

        public User? GetByUsername(string username)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Users WHERE Name = $name";
            command.Parameters.AddWithValue("$name", username);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            return new User
            {
                Id                  = Guid.Parse(reader.GetString(0)),
                Name                = reader.GetString(1),
                PasswordHash        = reader.GetString(2),
                CreatedAt           = DateTime.Parse(reader.GetString(3)),
                DefaultCurrencyCode = reader.GetString(4),
            };
        }

        public void Save(User user)
        {
            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            INSERT OR REPLACE INTO Users (Id, Name, PasswordHash, CreatedAt, DeafaultCurrencyCode)
            VALUES ($id, $name, $hash, $created, $currency);
            ";
            command.Parameters.AddWithValue("$id",       user.Id.ToString());
            command.Parameters.AddWithValue("$name",     user.Name);
            command.Parameters.AddWithValue("$hash",     user.PasswordHash);
            command.Parameters.AddWithValue("$created",  user.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("$currency", user.DefaultCurrencyCode);

            command.ExecuteNonQuery();
        }

        public List<User> GetAll()
        {
            var users = new List<User>();

            using var connection = new SqliteConnection($"Data Source={_dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Users";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id                  = Guid.Parse(reader.GetString(0)),
                    Name                = reader.GetString(1),
                    PasswordHash        = reader.GetString(2),
                    CreatedAt           = DateTime.Parse(reader.GetString(3)),
                    DefaultCurrencyCode = reader.GetString(4),
                });
            }

            return users;
        }
    }
}
