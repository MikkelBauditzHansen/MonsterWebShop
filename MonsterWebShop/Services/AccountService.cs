using MonsterWebShop.Models;
using MonsterWebShop.Repo;

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
        public bool Register(string username, string password)
        {
            // Tjek om brugernavnet allerede findes
            Account? existingAccount =
                accountRepo.GetAccountByUsername(username);

            if (existingAccount != null)
            {
                return false;
            }

            // Tjek password-policy
            if (!PasswordPolicy.IsValid(password))
            {
                return false;
            }

            // Hash password med Argon2
            string passwordHash =
                passwordHasher.HashPassword(password);

            // Opret ny customer
            Account account = new Account(
                0,
                username,
                passwordHash,
                "Customer"
            );

            // Gem account i databasen
            accountRepo.AddAccount(account);

            return true;
        }

        public Account? Login(string username, string password)
        {
            // Find account
            Account? account =
                accountRepo.GetAccountByUsername(username);

            if (account == null)
            {
                return null;
            }

            // Kontroller password
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
