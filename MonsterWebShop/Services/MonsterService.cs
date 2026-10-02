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

        public List<Monster> SearchMonsters(string searchText)
        {
            return _monsterRepo.SearchMonsters(searchText);
        }

        public List<Monster> SearchAndFilter(
            string searchText,
            string monsterType)
        {
            List<Monster> monsters;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                monsters = _monsterRepo.GetAllMonsters();
            }
            else
            {
                monsters = _monsterRepo.SearchMonsters(
                    searchText.Trim()
                );
            }

            if (monsterType == "Dragon")
            {
                monsters = monsters
                    .Where(m => m is Dragon)
                    .ToList();
            }

            if (monsterType == "Humanoid")
            {
                monsters = monsters
                    .Where(m => m is Humanoid)
                    .ToList();
            }

            if (monsterType == "Undead")
            {
                monsters = monsters
                    .Where(m => m is Undead)
                    .ToList();
            }

            return monsters;
        }

        public Monster AddMonster(Monster monster)
        {
            return _monsterRepo.AddMonster(monster);
        }

        public Monster? RemoveMonster(int id)
        {
            return _monsterRepo.RemoveMonster(id);
        }

        public Monster? UpdateMonster(
            int id,
            Monster monster)
        {
            return _monsterRepo.UpdateMonster(
                id,
                monster
            );
        }
    }
}