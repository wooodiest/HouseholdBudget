using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Core
{
    public interface IUserContext
    {
        User? CurrentUser { get; }

        bool IsLoggedIn { get; }

        void SetUser(User user);

        void Logout();
    }
}
