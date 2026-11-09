
namespace MonsterWebShop.Services
{
    public class AccessControlService
    {
        public bool CanViewProducts(string? role)
        {
            return true;
        }

        public bool CanAddToCart(string? role)
        {
            return role == "Customer";
        }

        public bool CanCheckout(string? role)
        {
            return role == "Customer";
        }

        public bool CanManageMonsters(string? role)
        {
            return role == "Admin";
        }

        public bool CanViewOrder(
            int? accountId,
            string? role,
            int orderOwnerId)
        {
            if (accountId == null)
            {
                return false;
            }

            return accountId.Value == orderOwnerId;
        }
    }
}
