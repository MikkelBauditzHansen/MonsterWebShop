namespace MonsterWebShop.Models
{
    public class CustomerAccount : Account
    {
        public override string? Role { get; set; } = "Customer";
        public CustomerAccount(int id, string username, string passwordHash) : base(id, username, passwordHash, "Admin")
        {
        }
    }
}
