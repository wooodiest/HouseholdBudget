using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Core
{
    public class UserContext : IUserContext
    {
        private static readonly User _defaultUser = new()
        {
            Id           = Guid.Empty,
            Name         = "Default User",
            Email        = string.Empty,
            PasswordHash = string.Empty,
            CreatedAt    = DateTime.UtcNow,
        };

        public User CurrentUser { get; private set; } = _defaultUser;

        public bool IsLoggedIn => CurrentUser.Id != Guid.Empty;

        public void SetUser(User user)
        {
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
        }

        public void Logout()
        {
            CurrentUser = _defaultUser;
        }
    }
}
