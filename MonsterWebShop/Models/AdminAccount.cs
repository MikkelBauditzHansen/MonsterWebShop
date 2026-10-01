namespace MonsterWebShop.Models
{
    public class AdminAccount : Account
    {
        public override string? Role { get; set; } = "Admin";

        public AdminAccount(
            int id,
            string username,
            string email,
            string passwordHash)
            : base(id, username, email, passwordHash, "Admin")
        {
        }
    }
}