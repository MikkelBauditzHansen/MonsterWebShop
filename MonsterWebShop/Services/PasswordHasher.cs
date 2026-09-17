using Isopoh.Cryptography.Argon2;

namespace MonsterWebShop.Services
{
    public class PasswordHasher
    {
        public string HashPassword(string password)
        {
            return Argon2.Hash(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return Argon2.Verify(hash, password);
        }
    }
}
