namespace VBSPOSS.Helpers.Interfaces
{
    /// <summary>
    /// Hỗ trợ mã hóa và giải mã dữ liệu, có thể sử dụng cho việc bảo vệ thông tin nhạy cảm như mật khẩu, token, hoặc dữ liệu cá nhân.
    /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
