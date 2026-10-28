
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Services;
using MonsterWebShop.Models;

namespace MonsterWebShop.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly AccountService accountService;
        private readonly CartCookieService cartCookieService;

        public LoginModel(
            AccountService accountService,
            CartCookieService cartCookieService)
        {
            this.accountService = accountService;
            this.cartCookieService = cartCookieService;
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
            MonsterWebShop.Models.Account? account =
                accountService.Login(Username, Password);

            if (account == null)
            {
                Message =
                    "Forkert brugernavn eller adgangskode.";

                return Page();
            }

            // Fjern tidligere brugers Session
            HttpContext.Session.Clear();

            // Gem oplysninger for den nye bruger
            HttpContext.Session.SetInt32(
                "AccountID",
                account.Id);

            HttpContext.Session.SetString(
                "Username",
                account.Username ?? "");

            HttpContext.Session.SetString(
                "Role",
                account.Role ?? "");

            // Gendan kun denne brugers kurv
            cartCookieService.GetCart(HttpContext);

            return RedirectToPage("/Index");
        }
    }
}
