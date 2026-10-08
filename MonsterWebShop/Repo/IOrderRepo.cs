using MonsterWebShop.Models;

namespace MonsterWebShop.Repo
{
    public interface IOrderRepo
    {
        Order? GetOrderById(int orderId);

        Order? GetOrderByCheckoutToken(Guid checkoutToken);

        List<Order> GetOrdersByAccountId(int accountId);

        Order CreateOrder(Order order);
        bool CompletePayment(int orderId, int accountId);

    }
}