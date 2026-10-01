using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using System.Security.Cryptography;

namespace MonsterWebShop.Services
{
    public class AccountService
    {
        private readonly IAccountRepo accountRepo;
        private readonly PasswordHasher passwordHasher;

        public AccountService(
            IAccountRepo accountRepo,
            PasswordHasher passwordHasher)
        {
            this.accountRepo = accountRepo;
            this.passwordHasher = passwordHasher;
        }
        public bool ResetPassword(
    string token,
    string newPassword)
        {
            Account? account =
                accountRepo.GetAccountByResetToken(token);

            if (account == null)
            {
                return false;
            }

            if (account.PasswordResetTokenExpires == null)
            {
                return false;
            }

            if (account.PasswordResetTokenExpires < DateTime.UtcNow)
            {
                return false;
            }

            if (!PasswordPolicy.IsValid(newPassword))
            {
                return false;
            }

            string passwordHash =
                passwordHasher.HashPassword(newPassword);

            account.PasswordHash = passwordHash;

            account.PasswordResetToken = null;

            account.PasswordResetTokenExpires = null;

            accountRepo.UpdateAccount(account);

            return true;
        }
        public string? CreatePasswordResetToken(string email)
        {
            Account? account =
                accountRepo.GetAccountByEmail(email);

            if (account == null)
            {
                return null;
            }

            string token =
                Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(32)
                );

            account.PasswordResetToken = token;

            account.PasswordResetTokenExpires =
                DateTime.UtcNow.AddMinutes(30);

            accountRepo.UpdateAccount(account);

            return token;
        }
        public bool Register(
            string username,
            string email,
            string password)
        {
            Account? existingAccount =
                accountRepo.GetAccountByUsername(username);

            if (existingAccount != null)
            {
                return false;
            }

            Account? existingEmail =
                accountRepo.GetAccountByEmail(email);

            if (existingEmail != null)
            {
                return false;
            }

            if (!PasswordPolicy.IsValid(password))
            {
                return false;
            }

            string passwordHash =
                passwordHasher.HashPassword(password);

            Account account = new Account(
                0,
                username,
                email,
                passwordHash,
                "Customer"
            );

            accountRepo.AddAccount(account);

            return true;
        }

        public Account? Login(
            string username,
            string password)
        {
            Account? account =
                accountRepo.GetAccountByUsername(username);

            if (account == null)
            {
                return null;
            }

            bool passwordCorrect =
                passwordHasher.VerifyPassword(
                    password,
                    account.PasswordHash!
                );

            if (!passwordCorrect)
            {
                return null;
            }

            return account;
        }
    }
}