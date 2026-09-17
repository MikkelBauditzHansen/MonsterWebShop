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
        public string Password { get; set; } = "";

        public string Message { get; set; } = "";

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            bool success = accountService.Register(Username, Password);

            if (!success)
            {
                Message = "Brugernavnet findes allerede, eller adgangskoden er ugyldig.";
                return Page();
            }

            return RedirectToPage("/Account/Login");
        }
    }
}
