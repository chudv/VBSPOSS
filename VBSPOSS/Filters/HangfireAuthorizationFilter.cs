namespace VBSPOSS.Filters
{
    using Hangfire.Dashboard;

    public class HangfireAuthorizationFilter
    : IDashboardAuthorizationFilter
    {
        public bool Authorize(
            DashboardContext context)
        {
            return true;
        }
    }
}
