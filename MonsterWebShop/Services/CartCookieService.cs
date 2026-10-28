
using System.Text.Json;

namespace MonsterWebShop.Services
{
    public class CartCookieService
    {
        private const string CookieName = "MonsterCart";

        public List<int> GetCart(HttpContext context)
        {
            string? cookie = context.Request.Cookies[CookieName];

            if (string.IsNullOrEmpty(cookie))
            {
                return new List<int>();
            }

            try
            {
                List<int>? cart =
                    JsonSerializer.Deserialize<List<int>>(cookie);

                if (cart == null)
                {
                    return new List<int>();
                }

                return cart
                    .Where(id => id > 0)
                    .Take(100)
                    .ToList();
            }
            catch
            {
                return new List<int>();
            }
        }

        public void SaveCart(
            HttpContext context,
            List<int> cart)
        {
            string json = JsonSerializer.Serialize(cart);

            CookieOptions options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            };

            context.Response.Cookies.Append(
                CookieName,
                json,
                options);
        }

        public void ClearCart(HttpContext context)
        {
            context.Response.Cookies.Delete("MonsterCart");
        }
    }
}
