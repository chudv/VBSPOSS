using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBSPOSS.Helpers.Implements;
using VBSPOSS.Helpers.Interfaces;
using Xunit.Abstractions;

namespace VBSPOSS.UnitTests
{
    public class ConnectionStringProviderTests
    {
        private readonly ITestOutputHelper _output;

        public ConnectionStringProviderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetOSSConnectionString_Should_Return_Decrypted_Value()
        {
            // Nếu AesEncryptionService của bạn đọc key/iv từ appsettings thì cần có các giá trị này
            // Nếu đọc từ Environment Variable thì set trước
            Environment.SetEnvironmentVariable("APP_ENCRYPT_KEY", "12345678901234567890123456789012");
            Environment.SetEnvironmentVariable("APP_ENCRYPT_IV", "1234567890123456");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IEncryptionService, AesEncryptionService>();
            services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();

            var sp = services.BuildServiceProvider();

            var provider = sp.GetRequiredService<IConnectionStringProvider>();
            var connStr = provider.GetOSSConnectionString();

            _output.WriteLine("===== OSS CONNECTION STRING =====");
            _output.WriteLine(connStr);
            _output.WriteLine("================================");

            Assert.False(string.IsNullOrWhiteSpace(connStr));
            Assert.Contains("Server", connStr, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Database", connStr, StringComparison.OrdinalIgnoreCase);
        }
    }
}
