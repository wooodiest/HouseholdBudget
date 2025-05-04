using HouseholdBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.Core.Core
{
    public class LoginService : ILoginService
    {
        private readonly IUserContext _userContext;
        private readonly IUserStorage _userStorage;

        public LoginService(IUserContext userContext, IUserStorage userStorage)
        {
            _userContext = userContext;
           _userStorage  = userStorage;
        }

        public bool TryLogin(string username, string password)
        {
            var user = _userStorage.GetByUsername(username);
            if (user == null)
                return false;

            var hash = HashPassword(password);
            if (user.PasswordHash != hash)
                return false;

            _userContext.SetUser(user);
            return true;
        }

        public void Logout()
        {
            _userContext.Logout();
        }

        public User Register(string username, string password)
        {
            var user = new User
            {
                Id           = Guid.NewGuid(),
                Name         = username,
                PasswordHash = HashPassword(password),
                CreatedAt    = DateTime.UtcNow
            };

            _userStorage.Save(user);
            return user;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes        = Encoding.UTF8.GetBytes(password);
            var hash         = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}
