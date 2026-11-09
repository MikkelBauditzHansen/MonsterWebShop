using MonsterWebShop.Models;

namespace MonsterWebShop.Repo
{
    public interface IOrderRepo
    {
        Order? GetOrderById(int orderId);

        Order? GetOrderByCheckoutToken(
        Guid checkoutToken,
        int accountId);
        List<Order> GetOrdersByAccountId(int accountId);
        Order? GetOrderForCustomer(int orderId, int accountId);
        Order CreateOrder(
        Order order,
        int authenticatedAccountId);
        bool CompletePayment(int orderId, int accountId);

    }
}