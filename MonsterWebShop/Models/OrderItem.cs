namespace MonsterWebShop.Models
{
    public class OrderItem
    {
        public int OrderItemID { get; set; }

        public int OrderID { get; set; }

        public int? MonsterID { get; set; }

        public string MonsterName { get; set; } = "";

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}