namespace MonsterWebShop.Models
{
    public enum UndeadType
    {
        Zombie,
        Vampire,
        Skeleton,
        Ghost
    }
    public class Undead : Monster
    {
        public UndeadType Type { get; set; }

        public Undead(string? name, string? color, string? imagePath, int age, UndeadType type)
            : base(name, color, imagePath, age)
        {
            Type = type;
        }
    }
}
