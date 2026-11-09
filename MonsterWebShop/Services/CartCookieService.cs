
using System.Text.Json;

namespace MonsterWebShop.Services
{
    public class CartCookieService
    {
        private string GetCookieName(int accountId)
        {
            return "MonsterCart_" + accountId;
        }

        public List<int> GetCart(HttpContext context)
        {
            int? accountId =
                context.Session.GetInt32("AccountID");

            if (accountId == null)
            {
                return new List<int>();
            }

            string? cartOwner =
                context.Session.GetString("CartOwnerId");

            string? sessionCart =
                context.Session.GetString("Cart");

            // Brug kun Session hvis kurven tilhører
            // den aktuelle bruger
            if (cartOwner == accountId.Value.ToString()
                && !string.IsNullOrEmpty(sessionCart))
            {
                try
                {
                    List<int>? cart =
                        JsonSerializer.Deserialize<List<int>>(
                            sessionCart);

                    if (cart != null)
                    {
                        return cart;
                    }
                }
                catch
                {
                    // Hent fra cookie i stedet
                }
            }

            // Hent den aktuelle brugers cookie
            string cookieName =
                GetCookieName(accountId.Value);

            string? cookie =
                context.Request.Cookies[cookieName];

            List<int> cookieCart = new List<int>();

            if (!string.IsNullOrEmpty(cookie))
            {
                try
                {
                    List<int>? savedCart =
                        JsonSerializer.Deserialize<List<int>>(
                            cookie);

                    if (savedCart != null)
                    {
                        cookieCart = savedCart
                            .Where(id => id > 0)
                            .Take(100)
                            .ToList();
                    }
                }
                catch
                {
                    cookieCart = new List<int>();
                }
            }

            // Gendan Session for den rigtige bruger
            context.Session.SetString(
                "CartOwnerId",
                accountId.Value.ToString());

            context.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cookieCart));

            return cookieCart;
        }

        public void SaveCart(
            HttpContext context,
            List<int> cart)
        {
            int? accountId =
                context.Session.GetInt32("AccountID");

            if (accountId == null)
            {
                return;
            }

            // Begræns hvor meget der kan gemmes
            List<int> validCart = cart
                .Where(id => id > 0)
                .Take(100)
                .ToList();

            string json =
                JsonSerializer.Serialize(validCart);

            // Gem i Session
            context.Session.SetString(
                "CartOwnerId",
                accountId.Value.ToString());

            context.Session.SetString(
                "Cart",
                json);

            // Gem i brugerens egen cookie
            CookieOptions options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            };

            context.Response.Cookies.Append(
                GetCookieName(accountId.Value),
                json,
                options);
        }

        public void ClearCart(HttpContext context)
        {
            int? accountId =
                context.Session.GetInt32("AccountID");

            context.Session.Remove("Cart");
            context.Session.Remove("CartOwnerId");

            if (accountId != null)
            {
                context.Response.Cookies.Delete(
                    GetCookieName(accountId.Value));
            }
        }
    }
}
