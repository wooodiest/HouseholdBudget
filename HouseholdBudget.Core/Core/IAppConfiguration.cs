namespace HouseholdBudget.Core.Core
{
    public interface IAppConfiguration
    {
        string GetValue(string key);

        string? TryGetValue(string key);
    }

}
