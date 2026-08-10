using System.Text.RegularExpressions;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers
{
    public static class PasswordPolicyHelper
    {
        public static bool IsValid(string password)
        {
            // Minimum 8 characters
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8)
                return false;

            // At least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // At least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // At least one number
            if (!Regex.IsMatch(password, @"[0-9]"))
                return false;

            // At least one special character
            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return false;

            return true;
        }
    }
}