namespace MonsterWebShop.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public virtual string? Role { get; set; }
        public Account(int id, string username, string password, string role)
        {
            Id = id;
            Username = username;
            Password = password;
            Role = role;
        }
    }
}
