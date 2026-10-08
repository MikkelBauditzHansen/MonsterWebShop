namespace MonsterWebShop.Models
{
    public class Order
    {
        public int OrderID { get; set; }

        public int AccountID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string PaymentStatus { get; set; } = "Pending";

        public Guid CheckoutToken { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}