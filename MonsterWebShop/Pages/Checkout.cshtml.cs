
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Repo;
using System.Text.Json;

namespace MonsterWebShop.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly IMonsterRepo _monsterRepo;
        private readonly IOrderRepo _orderRepo;

        public CheckoutModel(
            IMonsterRepo monsterRepo,
            IOrderRepo orderRepo)
        {
            _monsterRepo = monsterRepo;
            _orderRepo = orderRepo;
        }

        public List<Monster> MonstersInCart { get; set; } = new();

        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; } = 0;
        public decimal Total => Subtotal + Shipping;

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("AccountID") == null)
                return RedirectToPage("/Account/Login");

            if (!LoadCart())
                return RedirectToPage("/Cart");

            // Samme token bruges til denne checkout,
            // også hvis siden genindlæses.
            if (HttpContext.Session.GetString("CheckoutToken") == null)
            {
                HttpContext.Session.SetString(
                    "CheckoutToken",
                    Guid.NewGuid().ToString());
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            int? accountId =
                HttpContext.Session.GetInt32("AccountID");

            if (accountId == null)
                return RedirectToPage("/Account/Login");

            string? tokenString =
                HttpContext.Session.GetString("CheckoutToken");

            if (!Guid.TryParse(tokenString, out Guid token))
                return RedirectToPage("/Cart");

            // Undgå at oprette samme checkout igen.
            Order? existingOrder =
                _orderRepo.GetOrderByCheckoutToken(token);


            if (existingOrder != null)
            {
                if (existingOrder.AccountID != accountId.Value)
                    return Forbid();

                if (existingOrder.PaymentStatus == "Pending")
                {
                    _orderRepo.CompletePayment(
                        existingOrder.OrderID,
                        accountId.Value);
                }

                // Kontrollér den aktuelle status i databasen
                Order? updatedOrder =
                    _orderRepo.GetOrderById(existingOrder.OrderID);

                if (updatedOrder?.PaymentStatus != "Paid")
                {
                    ModelState.AddModelError(
                        "",
                        "Betalingen kunne ikke gennemføres.");

                    LoadCart();
                    return Page();
                }

                // Ordren er allerede betalt.
                // Ryd kun den checkout, der hører til ordren.
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("CheckoutToken");

                return RedirectToPage(
                    "/OrderConfirmation",
                    new { orderId = existingOrder.OrderID });
            }

            if (!LoadCart())
                return RedirectToPage("/Cart");

            var order = new Order
            {
                AccountID = accountId.Value,
                CheckoutToken = token,
                OrderItems = MonstersInCart.Select(monster =>
                    new OrderItem
                    {
                        MonsterID = monster.Id,
                        Quantity = 1
                    }).ToList()
            };

            // Priserne genberegnes i repositoryet.
            Order createdOrder = _orderRepo.CreateOrder(order);

            bool paymentSuccessful =
                _orderRepo.CompletePayment(
                    createdOrder.OrderID,
                    accountId.Value);

            if (!paymentSuccessful)
            {
                ModelState.AddModelError(
                    "",
                    "Betalingen kunne ikke gennemføres.");

                return Page();
            }

            // Tøm kurven efter gennemført betaling.
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CheckoutToken");

            return RedirectToPage(
                "/OrderConfirmation",
                new { orderId = createdOrder.OrderID });
        }

        private bool LoadCart()
        {
            string? cartJson =
                HttpContext.Session.GetString("Cart");

            if (string.IsNullOrWhiteSpace(cartJson))
                return false;

            List<int> cart;

            try
            {
                cart = JsonSerializer.Deserialize<List<int>>(cartJson)
                       ?? new List<int>();
            }
            catch (JsonException)
            {
                return false;
            }

            if (cart.Count == 0)
                return false;

            MonstersInCart.Clear();
            Subtotal = 0;

            foreach (int monsterId in cart)
            {
                Monster? monster =
                    _monsterRepo.GetMonsterById(monsterId);

                if (monster == null)
                    return false;

                MonstersInCart.Add(monster);

                Subtotal += decimal.Round(
                    Convert.ToDecimal(monster.Price),
                    2,
                    MidpointRounding.AwayFromZero);
            }

            return true;
        }
    }
}
