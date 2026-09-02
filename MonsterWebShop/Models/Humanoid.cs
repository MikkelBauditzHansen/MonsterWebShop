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

       public Humanoid(int id, string? name, string? color, string? imagePath, int age, HumanoidType type)
            : base(id, name, color, imagePath, age)
        {
            Type = type;
        }
    }
}
