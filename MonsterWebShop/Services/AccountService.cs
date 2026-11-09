using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using System.Security.Cryptography;

namespace MonsterWebShop.Services
{
    public class AccountService
    {
        private readonly IAccountRepo accountRepo;
        private readonly PasswordHasher passwordHasher;
        private readonly ILogRepo logRepo;

        public AccountService(
            IAccountRepo accountRepo,
            PasswordHasher passwordHasher,
            ILogRepo logRepo)
        {
            this.accountRepo = accountRepo;
            this.passwordHasher = passwordHasher;
            this.logRepo = logRepo;
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

            // Kun kunder må nulstille adgangskoden
            if (account.Role != "Customer")
            {
                return false;
            }

            // Kontroller at token har en udløbsdato
            if (account.PasswordResetTokenExpires == null)
            {
                return false;
            }

            // Kontroller at token ikke er udløbet
            if (account.PasswordResetTokenExpires <= DateTime.UtcNow)
            {
                return false;
            }

            // Kontroller at adgangskoden opfylder kravene
            if (!PasswordPolicy.IsValid(newPassword))
            {
                return false;
            }

            // Hash den nye adgangskode
            string passwordHash =
                passwordHasher.HashPassword(newPassword);

            account.PasswordHash = passwordHash;

            // Fjern token efter brug
            account.PasswordResetToken = null;
            account.PasswordResetTokenExpires = null;

            // Gem ændringerne i databasen
            accountRepo.UpdateAccount(account);

            return true;
        }


        public string? CreatePasswordResetToken(string email)
        {
            Account? account =
                accountRepo.GetAccountByEmail(email);

            // Kontroller at kontoen eksisterer
            if (account == null)
            {
                return null;
            }

            // Kun kunder må nulstille adgangskoden
            if (account.Role != "Customer")
            {
                return null;
            }

            // Generer et sikkert token
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
            string ip,
            string? sessionId = null)
        {
            Account? account =
                accountRepo.GetAccountByUsername(username);

            if (account == null)
            {
                LogLogin(ip, username, false, sessionId);
                return null;
            }

            bool passwordCorrect =
                passwordHasher.VerifyPassword(
                    password,
                    account.PasswordHash!
                );

            if (!passwordCorrect)
            {
                LogLogin(ip, username, false, sessionId);
                return null;
            }

            LogLogin(ip, username, true, sessionId);

            return account;
        }

        private void LogLogin(
            string ip,
            string username,
            bool success,
            string? sessionId)
        {
            try
            {
                string description = success
                    ? $"Succesfuldt login for '{username}'"
                    : $"Mislykket login for '{username}'";

                if (description.Length > 255)
                {
                    description = description.Substring(0, 255);
                }

                Log log = new Log
                {
                    IPAddress = string.IsNullOrWhiteSpace(ip)
                        ? "unknown"
                        : ip,
                    WhatHappened = description,
                    SessionID = sessionId,
                    IncidentType = success
                        ? "LoginSuccess"
                        : "LoginFailed"
                };

                logRepo.CreateLog(log);
            }
            catch
            {
                
            }
        }

    }
}