using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Services;

namespace MonsterWebShop.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MonsterService _monsterService;

        public IndexModel(MonsterService monsterService)
        {
            _monsterService = monsterService;
        }

        public List<Monster> Monsters { get; set; }
            = new List<Monster>();

        [BindProperty(SupportsGet = true)]
        public string SearchText { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string MonsterType { get; set; } = "";

        public int CartCount { get; set; }

        public void OnGet()
        {
            Monsters =
                _monsterService.SearchAndFilter(
                    SearchText,
                    MonsterType
                );

            string cart =
                HttpContext.Session.GetString("Cart") ?? "";

            if (string.IsNullOrEmpty(cart))
            {
                CartCount = 0;
            }
            else
            {
                CartCount =
                    cart.Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries
                    ).Length;
            }
        }

        public IActionResult OnPostAddToCart(
            int monsterId)
        {
            string cart =
                HttpContext.Session.GetString("Cart") ?? "";

            if (string.IsNullOrEmpty(cart))
            {
                cart = monsterId.ToString();
            }
            else
            {
                cart += "," + monsterId;
            }

            HttpContext.Session.SetString(
                "Cart",
                cart
            );

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(
            int monsterId)
        {
            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToPage();
            }

            _monsterService.RemoveMonster(monsterId);

            return RedirectToPage();
        }
    }
}