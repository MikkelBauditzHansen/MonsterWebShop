namespace MonsterWebShop.Models
{
    public enum HumanoidType 
    {
        Elf,
        Troll,
        Orc,
        Goblin
    }
    public class Humanoid : Monster
    {
        public HumanoidType Type { get; set; }

       public Humanoid(string? name, string? color, string? imagePath, int age, double price, HumanoidType type)
            : base(name, color, imagePath, age, price)
        {
            Type = type;
        }
    }
}
