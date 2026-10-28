
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using MonsterWebShop.Services;

namespace MonsterWebShop.Pages
{
    public class CartModel : PageModel
    {
        private readonly IMonsterRepo _monsterRepo;
        private readonly CartCookieService _cartCookieService;

        public List<Monster> MonstersInCart { get; set; } = new();

        public double Subtotal { get; set; }

        public double Shipping { get; set; }

        public double Total { get; set; }

        public CartModel(
            IMonsterRepo monsterRepo,
            CartCookieService cartCookieService)
        {
            _monsterRepo = monsterRepo;
            _cartCookieService = cartCookieService;
        }

        public void OnGet()
        {
            LoadCart();
        }

        public IActionResult OnPostRemove(int monsterId)
        {
            if (HttpContext.Session.GetInt32("AccountID") == null)
            {
                return RedirectToPage("/Account/Login");
            }

            List<int> cart =
                _cartCookieService.GetCart(HttpContext);

            cart.Remove(monsterId);

            _cartCookieService.SaveCart(
                HttpContext,
                cart);

            return RedirectToPage();
        }

        private void LoadCart()
        {
            List<int> cart =
                _cartCookieService.GetCart(HttpContext);

            foreach (int id in cart)
            {
                Monster? monster =
                    _monsterRepo.GetMonsterById(id);

                if (monster != null)
                {
                    MonstersInCart.Add(monster);

                    Subtotal += monster.Price;
                }
            }

            Shipping = 0;

            Total = Subtotal + Shipping;
        }
    }
}
