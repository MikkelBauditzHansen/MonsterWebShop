namespace MonsterWebShop.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        public virtual string? Role { get; set; }

        public int? AdminID { get; set; }

        public int? CustomerID { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpires { get; set; }

        public Account(
            int id,
            string username,
            string email,
            string passwordHash,
            string role)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }
    }
}