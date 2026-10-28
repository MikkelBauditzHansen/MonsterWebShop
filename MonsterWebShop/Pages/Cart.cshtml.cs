
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using MonsterWebShop.Services;
using System.Text.Json;

namespace MonsterWebShop.Pages
{
    public class CartModel : PageModel
    {
        private readonly IMonsterRepo _monsterRepo;
        private readonly CartCookieService _cartCookieService;
        private readonly AccessControlService _accessControl;

        public List<Monster> MonstersInCart { get; set; } = new();

        public double Subtotal { get; set; }

        public double Shipping { get; set; }

        public double Total { get; set; }

        public CartModel(
            IMonsterRepo monsterRepo,
            CartCookieService cartCookieService,
            AccessControlService accessControlService)
        {
            _monsterRepo = monsterRepo;
            _cartCookieService = cartCookieService;
            _accessControl = accessControlService;
        }

        public IActionResult OnGet()
        {
            int? accountId = HttpContext.Session.GetInt32("AccountID");
            string? role = HttpContext.Session.GetString("Role");

            if (accountId == null ||
                !_accessControl.CanAddToCart(role))
            {
                return StatusCode(403);
            }

            LoadCart();
            return Page();
        }

        public IActionResult OnPostRemove(int monsterId)
        {
            List<int> cart = GetCart();

            cart.Remove(monsterId);

            SaveCart(cart);

            return RedirectToPage();
        }

        // Hent kurven fra Session eller cookie
        private List<int> GetCart()
        {
            string? cartJson =
                HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                try
                {
                    List<int>? cart =
                        JsonSerializer.Deserialize<List<int>>(cartJson);

                    return cart ?? new List<int>();
                }
                catch
                {
                    // Hvis Session indeholder ugyldig JSON,
                    // forsøger vi at hente kurven fra cookien.
                }
            }

            // Session er tom, så hent fra cookie
            List<int> cookieCart =
                _cartCookieService.GetCart(HttpContext);

            // Gendan kurven i Session
            HttpContext.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cookieCart));

            return cookieCart;
        }

        // Gem kurven i både Session og cookie
        private void SaveCart(List<int> cart)
        {
            HttpContext.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cart));

            _cartCookieService.SaveCart(
                HttpContext,
                cart);
        }

        private void LoadCart()
        {
            List<int> cart = GetCart();

            foreach (int id in cart)
            {
                Monster? monster =
                    _monsterRepo.GetMonsterById(id);

                if (monster != null)
                {
                    MonstersInCart.Add(monster);

                    // Læg monsterets pris til subtotal
                    Subtotal += monster.Price;
                }
            }

            // Gratis levering
            Shipping = 0;

            Total = Subtotal + Shipping;
        }
    }
}
