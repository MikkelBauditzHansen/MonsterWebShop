using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Services;
using MonsterWebShop.Models;

namespace MonsterWebShop.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly AccountService accountService;

        public LoginModel(AccountService accountService)
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
            MonsterWebShop.Models.Account? account = accountService.Login(Username, Password,Convert.ToString(this.HttpContext.Connection.LocalIpAddress));

            if (account == null)
            {
                Message = "Forkert brugernavn eller adgangskode.";
                return Page();
            }
            HttpContext.Session.SetInt32("AccountID", account.Id);
            HttpContext.Session.SetString("Username", account.Username);
            HttpContext.Session.SetString("Role", account.Role);
            return RedirectToPage("/Index");
        }
    }
}
