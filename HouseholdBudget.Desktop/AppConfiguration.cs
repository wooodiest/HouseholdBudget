using HouseholdBudget.Core.Core;

namespace HouseholdBudget.Desktop
{
    public class AppConfiguration : IAppConfiguration
    {
        private readonly Dictionary<string, string> _settings = new()
        {
            { "UserStorageFile", "householdBudgetUsers.db" },
            { "DatabaseFile",    "householdBudget.db"      }
        };

        public string GetValue(string key)
        {
            if (!_settings.TryGetValue(key, out var value))
                throw new KeyNotFoundException($"Key '{key}' not found in configuration.");

            return value;
        }

        public string? TryGetValue(string key) =>
            _settings.TryGetValue(key, out var value) ? value : null;
    }
}
