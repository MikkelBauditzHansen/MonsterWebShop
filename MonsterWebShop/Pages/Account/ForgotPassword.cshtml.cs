using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Services;

namespace MonsterWebShop.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly AccountService accountService;
        private readonly EmailService emailService;

        public ForgotPasswordModel(
            AccountService accountService,
            EmailService emailService)
        {
            this.accountService = accountService;
            this.emailService = emailService;
        }

        [BindProperty]
        public string Email { get; set; } = "";

        public string Message { get; set; } = "";

        public void OnGet()
        {
        }

        public void OnPost()
        {
            string? token =
                accountService.CreatePasswordResetToken(Email);

            if (token != null)
            {
                string resetLink =
                    $"https://localhost:7261/Account/ResetPassword?token={token}";

                bool emailSent =
                    emailService.SendPasswordResetEmail(
                        Email,
                        resetLink
                    );

                if (!emailSent)
                {
                    Message = "Mailen kunne ikke sendes.";
                    return;
                }
            }

            Message =
                "Hvis emailen findes, er der sendt et link til nulstilling af adgangskoden.";
        }
    }
}