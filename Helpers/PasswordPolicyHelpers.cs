using System.Text.RegularExpressions;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers
{
    public static class PasswordPolicyHelper
    {
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8)
                return false;

            if (!Regex.IsMatch(password, "[A-Z]"))
                return false;

            if (!Regex.IsMatch(password, "[a-z]"))
                return false;

            if (!Regex.IsMatch(password, "[0-9]"))
                return false;

            if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                return false;

            return true;
        }

        // Optional: keeps compatibility with your Forgot Password code
        public static bool IsValid(string password)
        {
            return IsStrongPassword(password);
        }
    }
}