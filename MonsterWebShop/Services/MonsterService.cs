using MonsterWebShop.Models;
using MonsterWebShop.Repo;

namespace MonsterWebShop.Services
{
    public class MonsterService
    {
        private readonly IMonsterRepo _monsterRepo;

        public MonsterService(IMonsterRepo monsterRepo)
        {
            _monsterRepo = monsterRepo;
        }

        public List<Monster> GetAllMonsters()
        {
            return _monsterRepo.GetAllMonsters();
        }

        public Monster? GetMonsterById(int id)
        {
            return _monsterRepo.GetMonsterById(id);
        }

        public Monster AddMonster(Monster monster)
        {
            return _monsterRepo.AddMonster(monster);
        }

        public Monster? RemoveMonster(int id)
        {
            return _monsterRepo.RemoveMonster(id);
        }

        public Monster? UpdateMonster(int id, Monster monster)
        {
            return _monsterRepo.UpdateMonster(id, monster);
        }
    }
}