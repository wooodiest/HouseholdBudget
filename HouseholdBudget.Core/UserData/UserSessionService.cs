namespace HouseholdBudget.Core.UserData
{
    /// <summary>
    /// Default implementation of <see cref="IUserSessionService"/> that manages user login sessions.
    /// </summary>
    public class UserSessionService : IUserSessionService
    {
        private readonly IUserAuthenticator _authenticator;
        private readonly IUserContext _userContext;

        public UserSessionService(IUserAuthenticator authenticator, IUserContext userContext)
        {
            _authenticator = authenticator;
            _userContext   = userContext;
        }

        /// <inheritdoc/>
        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await _authenticator.AuthenticateAsync(email, password);
            if (user == null)
                return false;

            _userContext.SetUser(user);
            return true;
        }

        /// <inheritdoc/>
        public void Logout()
        {
            _userContext.Clear();
        }

        /// <inheritdoc/>
        public User? GetUser() => _userContext.CurrentUser;

        /// <inheritdoc/>
        public bool IsAuthenticated => _userContext.CurrentUser != null;
    }
}
