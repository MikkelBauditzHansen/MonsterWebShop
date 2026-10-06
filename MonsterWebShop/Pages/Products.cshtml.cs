using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Services;
using System.Text.Json;

namespace MonsterWebShop.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly MonsterService _monsterService;

        public ProductsModel(MonsterService monsterService)
        {
            _monsterService = monsterService;
        }

        public List<Monster> Monsters { get; set; } = new();

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

            string? cartJson =
                HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                CartCount = 0;
                return;
            }

            try
            {
                List<int> cart =
                    JsonSerializer.Deserialize<List<int>>(cartJson)
                    ?? new List<int>();

                CartCount = cart.Count;
            }
            catch
            {
                HttpContext.Session.Remove("Cart");
                CartCount = 0;
            }
        }

        public IActionResult OnPostAddToCart(
            int monsterId)
        {
            string? cartJson =
                HttpContext.Session.GetString("Cart");

            List<int> cart;

            if (string.IsNullOrEmpty(cartJson))
            {
                cart = new List<int>();
            }
            else
            {
                try
                {
                    cart =
                        JsonSerializer.Deserialize<List<int>>(cartJson)
                        ?? new List<int>();
                }
                catch
                {
                    cart = new List<int>();
                }
            }

            cart.Add(monsterId);

            HttpContext.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cart)
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