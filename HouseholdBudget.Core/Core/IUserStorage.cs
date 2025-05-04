using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Core
{
    public interface IUserStorage
    {
        User? GetByUsername(string username);

        void Save(User user);

        List<User> GetAll();
    }
}
