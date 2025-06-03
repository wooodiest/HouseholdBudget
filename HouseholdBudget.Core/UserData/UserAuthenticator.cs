using System.ComponentModel.DataAnnotations;
using HouseholdBudget.Core.Data;
using Microsoft.AspNetCore.Identity;

namespace HouseholdBudget.Core.UserData
{
    /// <summary>
    /// Default implementation of <see cref="IUserAuthenticator"/> that verifies user credentials
    /// against the stored password hash.
    /// </summary>
    public class UserAuthenticator : IUserAuthenticator
    {
        private readonly IBudgetRepository _repository;
        private readonly IPasswordHasher<User> _passwordHasher;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthenticator"/> class.
        /// </summary>
        /// <param name="repository">The repository used to retrieve user records.</param>
        /// <param name="passwordHasher">The service used to hash and verify passwords.</param>
        public UserAuthenticator(IBudgetRepository repository, IPasswordHasher<User> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        /// <inheritdoc/>
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var emailErrors    = User.ValidateEmail(email);
            var passwordErrors = string.IsNullOrWhiteSpace(password) ? new List<string> { "Password is required." } : new List<string>();

            var allErrors = emailErrors.Concat(passwordErrors).ToList();
            if (allErrors.Count > 0)
                throw new ValidationException(string.Join("; ", allErrors));

            var user = await _repository.GetUserByEmailAsync(email.Trim().ToLowerInvariant());
            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            return result == PasswordVerificationResult.Success ? user : null;
        }

        /// <inheritdoc/>
        public async Task<User> RegisterAsync(string name, string email, string password, string defaultCurrencyCode)
        {
            var errors = new List<string>();
            errors.AddRange(User.ValidateName(name));
            errors.AddRange(User.ValidateEmail(email));
            errors.AddRange(User.ValidateCurrencyCode(defaultCurrencyCode));
            errors.AddRange(ValidatePassword(password));

            if (await _repository.GetUserByEmailAsync(email) is not null)
                errors.Add("User with this email already exists.");

            if (errors.Any())
                throw new ValidationException(string.Join("; ", errors));

            var user = User.Create(
                name:                name,
                email:               email,
                passwordHash:        _passwordHasher.HashPassword(null!, password),
                defaultCurrencyCode: defaultCurrencyCode
            );

            await _repository.AddUserAsync(user);
            await _repository.SaveChangesAsync();

            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _repository.GetUserByEmailAsync(email) is not null;   
        }

        private static IReadOnlyList<string> ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
                errors.Add("Password is required.");
            else
            {
                if (password.Length < 8)
                    errors.Add("Password must be at least 8 characters long.");
                if (!password.Any(char.IsUpper))
                    errors.Add("Password must contain at least one uppercase letter.");
                if (!password.Any(char.IsLower))
                    errors.Add("Password must contain at least one lowercase letter.");
                if (!password.Any(char.IsDigit))
                    errors.Add("Password must contain at least one digit.");
                if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                    errors.Add("Password must contain at least one special character.");
            }

            return errors;
        }
 
    }
}
