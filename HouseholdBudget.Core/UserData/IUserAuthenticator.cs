namespace HouseholdBudget.Core.UserData
{
    /// <summary>
    /// Defines the contract for authenticating users using credentials.
    /// </summary>
    public interface IUserAuthenticator
    {
        /// <summary>
        /// Attempts to authenticate a user using the provided email and password.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The plaintext password provided by the user.</param>
        /// <returns>
        /// The authenticated <see cref="User"/> if successful; otherwise, <c>null</c>.
        /// </returns>
        Task<User?> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Registers a new user with the specified data.
        /// </summary>
        /// <param name="name">The user's full name.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's plain-text password.</param>
        /// <param name="defaultCurrencyCode">The user's preferred currency code.</param>
        /// <returns>The newly registered user.</returns>
        /// <exception cref="ValidationException">Thrown when input data is invalid or user already exists.</exception>
        Task<User> RegisterAsync(string name, string email, string password, string defaultCurrencyCode);
    }
}
