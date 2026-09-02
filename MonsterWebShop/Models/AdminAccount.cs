namespace MonsterWebShop.Models
{
    public class AdminAccount : Account
    {
        public override string? Role { get; set; } = "Admin";
        public AdminAccount(int id, string username, string password, string role) : base(id, username, password, role)
        {
        }

    }
}
