using Xunit.Abstractions;

namespace VBSPOSS.UnitTests
{
    public class EncryptionTests
    {
        private readonly ITestOutputHelper _output;

        public EncryptionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Generate_Encrypted_ConnectionString()
        {
            // Arrange
            var plainText = "User Id=VBSPOSS;Password=vbsposs;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.63.48.181)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=VBSPUAT)));";
            //var plainText = "Server=10.63.48.63;Database=VBSPOSS;User Id=sa;Password=Sql2017;MultipleActiveResultSets=true";


            var helper = new TestEncryptionHelper();

            // Act
            var encryptedText = helper.Encrypt(plainText);
            var decryptedText = helper.Decrypt(encryptedText);

            // Output
            _output.WriteLine("===== CONNECTION STRING ĐÃ MÃ HÓA =====");
            _output.WriteLine(encryptedText);
            _output.WriteLine("=======================================");

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(encryptedText));
            Assert.Equal(plainText, decryptedText);
        }
    }
}