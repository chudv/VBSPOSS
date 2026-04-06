namespace VBSPOSS.Helpers.Interfaces
{
    public interface IConnectionStringProvider
    {
        string GetOracleConnectionString();

        string GetOSSConnectionString();

    }
}
