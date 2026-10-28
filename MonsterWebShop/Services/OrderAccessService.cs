
using MonsterWebShop.Models;
using MonsterWebShop.Repo;

namespace MonsterWebShop.Services
{
    public class OrderAccessService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly AccessControlService _accessControl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderAccessService(
            IOrderRepo orderRepo,
            AccessControlService accessControl,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderRepo = orderRepo;
            _accessControl = accessControl;
            _httpContextAccessor = httpContextAccessor;
        }

        public List<Order> GetMyOrders()
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            int? accountId = session?.GetInt32("AccountID");
            string? role = session?.GetString("Role");

            if (accountId == null ||
                !_accessControl.CanCheckout(role))
            {
                throw new UnauthorizedAccessException(
                    "Du har ikke adgang til ordrer.");
            }

            return _orderRepo.GetOrdersByAccountId(
                accountId.Value);
        }
        public bool CanAccessOrder(int orderOwnerId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            int? accountId = session?.GetInt32("AccountID");
            string? role = session?.GetString("Role");

            return _accessControl.CanViewOrder(
                accountId,
                role,
                orderOwnerId);
        }
    }
}
