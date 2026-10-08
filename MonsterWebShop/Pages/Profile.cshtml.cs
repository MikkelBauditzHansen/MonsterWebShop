
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using MonsterWebShop.Services;

namespace MonsterWebShop.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly IAccountRepo _accountRepo;
        private readonly AccountService _accountService;
        private readonly IOrderRepo _orderRepo;

        public List<Order> Orders { get; set; } = new();

        public ProfileModel(
    IAccountRepo accountRepo,
    AccountService accountService,
    IOrderRepo orderRepo)
        {
            _accountRepo = accountRepo;
            _accountService = accountService;
            _orderRepo = orderRepo;
        }

        public Models.Account Account { get; set; } = null!;

        [BindProperty]
        public string CurrentPassword { get; set; } = "";

        [BindProperty]
        public string NewPassword { get; set; } = "";

        [BindProperty]
        public string RepeatPassword { get; set; } = "";

        public string Message { get; set; } = "";

        public bool PasswordChanged { get; set; }

        public IActionResult OnGet()
        {
            int? accountId =
                HttpContext.Session.GetInt32("AccountID");

            if (accountId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Models.Account? account =
                _accountRepo.GetAccount(accountId.Value);

            if (account == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Account = account;

            Orders = _orderRepo.GetOrdersByAccountId(accountId.Value);

            return Page();
        }

        public IActionResult OnPostChangePassword()
        {
            int? accountId =
                HttpContext.Session.GetInt32("AccountID");

            if (accountId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Models.Account? account =
                _accountRepo.GetAccount(accountId.Value);

            if (account == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Account = account;

            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(RepeatPassword))
            {
                Message = "Alle adgangskodefelter skal udfyldes.";
                return Page();
            }

            if (NewPassword != RepeatPassword)
            {
                Message = "De nye adgangskoder er ikke ens.";
                return Page();
            }

            bool success = _accountService.ChangePassword(
                accountId.Value,
                CurrentPassword,
                NewPassword);

            if (!success)
            {
                Message =
                    "Adgangskoden kunne ikke ændres. Kontrollér din nuværende adgangskode og kravene til den nye.";

                return Page();
            }

            TempData["PasswordSuccess"] =
                "Din adgangskode er blevet ændret.";

            return RedirectToPage();
        }
    }
}
