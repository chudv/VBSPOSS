using VBSPOSS.Helpers.Interfaces;

namespace VBSPOSS.Helpers.Implements
{
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IEncryptionService _encryptionService;

        public ConnectionStringProvider(
            IConfiguration configuration,
            IEncryptionService encryptionService)
        {
            _configuration = configuration;
            _encryptionService = encryptionService;
        }

        public string GetOracleConnectionString()
        {
            var encrypted = _configuration.GetConnectionString("IntellectIDCConnection");

            if (string.IsNullOrWhiteSpace(encrypted))
                throw new InvalidOperationException("Không tìm thấy IntellectIDCConnection.");

            return _encryptionService.Decrypt(encrypted);
        }

        public string GetOSSConnectionString()
        {
            var encrypted = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(encrypted))
                throw new InvalidOperationException("Không tìm thấy DefaultConnection.");
            return _encryptionService.Decrypt(encrypted);
        }
    }
}
