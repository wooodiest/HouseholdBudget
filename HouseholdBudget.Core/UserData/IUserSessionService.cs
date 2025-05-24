namespace HouseholdBudget.Core.UserData
{
    /// <summary>
    /// Provides high-level operations for managing user login sessions.
    /// </summary>
    public interface IUserSessionService
    {
        /// <summary>
        /// Attempts to log in the user with the given credentials.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's plaintext password.</param>
        /// <returns><c>true</c> if authentication succeeded; otherwise <c>false</c>.</returns>
        Task<bool> LoginAsync(string email, string password);

        /// <summary>
        /// Logs out the current user and clears the user context.
        /// </summary>
        void Logout();

        /// <summary>
        /// Gets the currently logged-in user, if any.
        /// </summary>
        User? GetUser();

        /// <summary>
        /// Indicates whether a user is currently authenticated.
        /// </summary>
        bool IsAuthenticated { get; }
    }
}
