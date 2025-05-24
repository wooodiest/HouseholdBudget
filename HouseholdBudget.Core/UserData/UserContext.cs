namespace HouseholdBudget.Core.UserData
{
    /// <summary>
    /// Default implementation of <see cref="IUserContext"/> that holds user context data
    /// in memory for the duration of a request (should be registered as Scoped).
    /// </summary>
    public class UserContext : IUserContext
    {
        private User? _currentUser;

        /// <inheritdoc/>
        public User? CurrentUser => _currentUser;

        /// <inheritdoc/>
        public void SetUser(User user) => _currentUser = user;

        /// <inheritdoc/>
        public void Clear() => _currentUser = null;
    }
}
