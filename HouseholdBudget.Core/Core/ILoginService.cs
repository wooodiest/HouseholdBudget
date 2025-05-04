using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Core
{
    public interface ILoginService
    {
        bool TryLogin(string username, string password);

        void Logout();

        User Register(string username, string password);
    }
}
