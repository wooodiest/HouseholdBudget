namespace HouseholdBudget.Core.UserData
{
    /// <summary>
    /// Represents an abstraction for accessing the current authenticated user's context
    /// during a request lifecycle.
    /// </summary>
    public interface IUserContext
    {
        /// <summary>
        /// Gets the currently authenticated user, if any.
        /// </summary>
        User? CurrentUser { get; }

        /// <summary>
        /// Sets the current user for the context.
        /// This should only be called once per request, typically during authentication.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        void SetUser(User user);

        /// <summary>
        /// Clears the current user context (e.g., at the end of a request or logout).
        /// </summary>
        void Clear();
    }
}
