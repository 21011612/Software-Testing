namespace QuanLyQuanCafe.Helpers;

public static class PasswordHelper
{
    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password.Trim());

    public static bool Verify(string password, string stored)
    {
        if (string.IsNullOrWhiteSpace(stored) || string.IsNullOrEmpty(password))
            return false;

        password = password.Trim();
        stored = stored.Trim();

        if (stored.StartsWith("$2", StringComparison.Ordinal))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, stored);
            }
            catch
            {
                return false;
            }
        }

        return string.Equals(password, stored, StringComparison.Ordinal);
    }
}