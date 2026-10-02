using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using System.Text.Json;

namespace MonsterWebShop.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMonsterRepo _monsterRepo;

        public List<Monster> Monsters { get; set; } = new();

        public int CartCount { get; set; }

        public IndexModel(IMonsterRepo monsterRepo)
        {
            _monsterRepo = monsterRepo;
        }

        public void OnGet()
        {
            Monsters = _monsterRepo.GetAllMonsters();

            var cartJson = HttpContext.Session.GetString("Cart");

            if (cartJson != null)
            {
                var cart = JsonSerializer.Deserialize<List<int>>(cartJson);

                CartCount = cart?.Count ?? 0;
            }
        }
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();

            return RedirectToPage("/Index");
        }

        public IActionResult OnPostAddToCart(int monsterId)
        {
            var cartJson = HttpContext.Session.GetString("Cart");

            List<int> cart;

            if (cartJson == null)
            {
                cart = new List<int>();
            }
            else
            {
                cart = JsonSerializer.Deserialize<List<int>>(cartJson)
                       ?? new List<int>();
            }

            cart.Add(monsterId);

            HttpContext.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cart)
            );

            return RedirectToPage();
        }
        public IActionResult OnPostDelete(int monsterId)
        {
            string role = HttpContext.Session.GetString("Role");
            Monster monster = _monsterRepo.GetMonsterById(monsterId);
            if (role == "Admin" && monster != null)
            {
                _monsterRepo.RemoveMonster(monsterId);
            }
            return RedirectToPage("/index");
        }
    }
}