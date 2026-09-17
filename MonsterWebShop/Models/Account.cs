namespace MonsterWebShop.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public virtual string? Role { get; set; }
        public int? AdminID { get; set; }
        public int? CustomerID { get; set; }
        public Account(int id, string username, string passwordHash, string role)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
        }
    }
}
