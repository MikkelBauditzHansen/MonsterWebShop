namespace MonsterWebShop.Models
{
    public class Monster
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? ImagePath { get; set; }
        public int Age { get; set; }

        public Monster(string? name, string? color, string? imagePath, int age)
        {
            Name = name;
            Color = color;
            ImagePath = imagePath;
            Age = age;
        }
    }
}
