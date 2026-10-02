using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Services;

namespace MonsterWebShop.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly AccountService accountService;

        public ResetPasswordModel(
            AccountService accountService)
        {
            this.accountService = accountService;
        }

        [BindProperty]
        public string Token { get; set; } = "";

        [BindProperty]
        public string NewPassword { get; set; } = "";

        [BindProperty]
        public string RepeatPassword { get; set; } = "";

        public string Message { get; set; } = "";

        public void OnGet(string token)
        {
            Token = token;
        }

        public IActionResult OnPost()
        {
            if (NewPassword != RepeatPassword)
            {
                Message = "Adgangskoderne er ikke ens.";
                return Page();
            }

            bool success =
                accountService.ResetPassword(
                    Token,
                    NewPassword
                );

            if (!success)
            {
                Message =
                    "Linket er ugyldigt, udløbet eller adgangskoden opfylder ikke kravene.";

                return Page();
            }

            return RedirectToPage("/Account/Login");
        }
    }
}