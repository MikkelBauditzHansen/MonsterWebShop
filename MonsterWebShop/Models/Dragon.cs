namespace MonsterWebShop.Models
{
    public enum DragonType
    {
        Fire,
        Ice,
        Earth,
        Water
    }
    public class Dragon : Monster
    {
        public double WingSpan { get; set; }
        public DragonType Type { get; set; }

        public Dragon(string? name, string? color, string? imagePath, int age, double wingSpan, DragonType type)
            : base(name, color, imagePath, age)
        {
            WingSpan = wingSpan;
            Type = type;
        }
    }
}
