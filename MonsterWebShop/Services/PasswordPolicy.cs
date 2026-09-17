namespace MonsterWebShop.Services
{
    public class PasswordPolicy
    {
        private static readonly string[] ForbiddenPasswords =
        {
                "password",
                "password123",
                "12345678",
                "qwerty",
                "admin",
                "administrator",
                "welcome"
        };
        public static bool IsValid(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Minimum 8 tegn
            if (password.Length < 8)
                return false;

            // Maksimum 64 tegn
            if (password.Length > 64)
                return false;

            // Mindst ét stort bogstav
            if (!password.Any(char.IsUpper))
                return false;

            // Mindst ét specialtegn
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return false;

            return true;
        }
    }
}
