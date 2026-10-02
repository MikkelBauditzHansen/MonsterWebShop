using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using System.Text.Json;

namespace MonsterWebShop.Pages
{
    public class CartModel : PageModel
    {
        private readonly IMonsterRepo _monsterRepo;

        public List<Monster> MonstersInCart { get; set; } = new();

        public double Subtotal { get; set; }

        public double Shipping { get; set; }

        public double Total { get; set; }

        public CartModel(IMonsterRepo monsterRepo)
        {
            _monsterRepo = monsterRepo;
        }

        public void OnGet()
        {
            LoadCart();
        }

        public IActionResult OnPostRemove(int monsterId)
        {
            var cartJson = HttpContext.Session.GetString("Cart");

            if (cartJson != null)
            {
                var cart = JsonSerializer.Deserialize<List<int>>(cartJson)
                           ?? new List<int>();

                cart.Remove(monsterId);

                HttpContext.Session.SetString(
                    "Cart",
                    JsonSerializer.Serialize(cart)
                );
            }

            return RedirectToPage();
        }

        private void LoadCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");

            if (cartJson == null)
            {
                return;
            }

            var cart = JsonSerializer.Deserialize<List<int>>(cartJson)
                       ?? new List<int>();

            foreach (var id in cart)
            {
                var monster = _monsterRepo.GetMonsterById(id);

                if (monster != null)
                {
                    MonstersInCart.Add(monster);

                    // Læg monsterets pris til subtotal
                    Subtotal += monster.Price;
                }
            }

            // Eksempel: gratis levering
            Shipping = 0;

            Total = Subtotal + Shipping;
        }
    }
}