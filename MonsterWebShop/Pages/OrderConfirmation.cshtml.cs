
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Repo;

namespace MonsterWebShop.Pages
{
    public class OrderConfirmationModel : PageModel
    {
        private readonly IOrderRepo _orderRepo;

        public OrderConfirmationModel(IOrderRepo orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public Order Order { get; set; } = null!;

        public IActionResult OnGet(int orderId)
        {
            int? accountId =
                HttpContext.Session.GetInt32("AccountID");

            if (accountId == null)
                return RedirectToPage("/Account/Login");

            Order? order = _orderRepo.GetOrderById(orderId);

            if (order == null || order.AccountID != accountId.Value)
                return NotFound();

            if (order.PaymentStatus != "Paid")
                return RedirectToPage("/Cart");

            Order = order;

            return Page();
        }
    }
}
