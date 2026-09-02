using MonsterWebShop.Models;
namespace MonsterWebShop.Repo
{
    public class MonsterRepoList : IMonsterRepo
    {
        private List<Monster> _monsters = new List<Monster>();
        private int _nextId = 1;

        public MonsterRepoList()
        {
            // Initialize with some sample monsters
            AddMonster(new Dragon("Smaug", "Red", "/images/smaug.png", 100, 150, DragonType.Fire));
            AddMonster(new Dragon("Toothless", "Black", "/images/toothless.png", 20, 50, DragonType.Ice));
            AddMonster(new Dragon("Drogon", "Black and Red", "/images/drogon.png", 5, 100, DragonType.Earth));
        }
        public List<Monster> GetAllMonsters()
        {
            return _monsters;
        }
        public Monster? GetMonsterById(int id)
        {
            return _monsters.FirstOrDefault(m => m.Id == id);
        }
        public Monster AddMonster(Monster monster)
        {
            monster.Id = _nextId++;
            _monsters.Add(monster);
            return monster;
        }
        public Monster? RemoveMonster(int id)
        {
            var monster = GetMonsterById(id);
            if (monster != null)
            {
                _monsters.Remove(monster);
            }
            return monster;
        }
        public Monster? UpdateMonster(int id, Monster updatedMonster)
        {
            var monster = GetMonsterById(id);
            if (monster != null)
            {
                monster.Name = updatedMonster.Name;
                monster.Color = updatedMonster.Color;
                monster.ImagePath = updatedMonster.ImagePath;
                monster.Age = updatedMonster.Age;
                if (monster is Dragon dragon && updatedMonster is Dragon updatedDragon)
                {
                    dragon.WingSpan = updatedDragon.WingSpan;
                    dragon.Type = updatedDragon.Type;
                }
                if (monster is Undead undead && updatedMonster is Undead updatedUndead)
                {
                    undead.Type = updatedUndead.Type;
                }
                if (monster is Humanoid humanoid && updatedMonster is Humanoid updatedHumanoid)
                {
                    humanoid.Type = updatedHumanoid.Type;
                }
                return monster;
            }
            return monster;
        }
    }
}
