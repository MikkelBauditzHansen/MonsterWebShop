using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using System.Diagnostics;
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
    string password,
    string role)
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

            // Sørg for at rollen er gyldig
            if (role != "Admin" && role != "Customer")
            {
                return false;
            }

            string passwordHash =
                passwordHasher.HashPassword(password);

            Account account;

            if (role == "Admin")
            {
                account = new AdminAccount(
                    0,
                    username,
                    email,
                    passwordHash
                );
            }
            else
            {
                account = new CustomerAccount(
                    0,
                    username,
                    email,
                    passwordHash
                );
            }

            accountRepo.AddAccount(account);

            return true;
        }

        public bool ChangePassword(
            int accountId,
            string currentPassword,
            string newPassword)
        {
            // Hent den bruger, som er logget ind
            Account? account = accountRepo.GetAccount(accountId);

            if (account == null)
            {
                return false;
            }

            // Kontrollér den nuværende adgangskode
            bool currentPasswordCorrect =
                passwordHasher.VerifyPassword(
                    currentPassword,
                    account.PasswordHash!
                );

            if (!currentPasswordCorrect)
            {
                return false;
            }

            // Kontrollér at den nye adgangskode opfylder kravene
            if (!PasswordPolicy.IsValid(newPassword))
            {
                return false;
            }

            // Undgå at genbruge den nuværende adgangskode
            if (passwordHasher.VerifyPassword(
                newPassword,
                account.PasswordHash!))
            {
                return false;
            }

            // Hash den nye adgangskode
            account.PasswordHash =
                passwordHasher.HashPassword(newPassword);

            // Ugyldiggør eventuelle eksisterende reset-links
            account.PasswordResetToken = null;
            account.PasswordResetTokenExpires = null;

            // Gem ændringen i databasen
            accountRepo.UpdateAccount(account);

            return true;
        }

        public Account? Login(
            string username,
            string password,
            string ip)
        {
            Debug.WriteLine($"ip is {ip}");
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

        private void LogLogin()
        {

        }

    }
}