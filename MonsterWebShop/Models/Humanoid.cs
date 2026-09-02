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

       public Humanoid(string? name, string? color, string? imagePath, int age, HumanoidType type)
            : base(name, color, imagePath, age)
        {
            Type = type;
        }
    }
}
