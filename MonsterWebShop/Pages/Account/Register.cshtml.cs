using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Services;

namespace MonsterWebShop.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly AccountService accountService;

        public RegisterModel(AccountService accountService)
        {
            this.accountService = accountService;
        }

        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        [BindProperty]
        public string RepeatPassword { get; set; } = "";
        [BindProperty]
        public string Role { get; set; } = "";

        public string Message { get; set; } = "";

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (Password != RepeatPassword)
            {
                Message = "Adgangskoderne er ikke ens.";
                return Page();
            }

            bool success = accountService.Register(
                Username,
                Email,
                Password,
                Role
            );

            if (!success)
            {
                Message =
                    "Brugernavnet eller email findes allerede, eller adgangskoden er ugyldig.";

                return Page();
            }

            return RedirectToPage("/Account/Login");
        }
    }
}