using MonsterWebShop.Models;
using MonsterWebShop.Repo;

namespace MonsterWebShop.Services
{
    public class MonsterService
    {
        private readonly IMonsterRepo _monsterRepo;
        private readonly AccessControlService _accessControl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MonsterService(
            IMonsterRepo monsterRepo,
            AccessControlService accessControl,
            IHttpContextAccessor httpContextAccessor)
        {
            _monsterRepo = monsterRepo;
            _accessControl = accessControl;
            _httpContextAccessor = httpContextAccessor;
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
            RequireAdmin();

            return _monsterRepo.AddMonster(monster);
        }

        public Monster? RemoveMonster(int id)
        {
            RequireAdmin();

            return _monsterRepo.RemoveMonster(id);
        }

        public Monster? UpdateMonster(
        int id,
        Monster monster)
        {
            RequireAdmin();

            return _monsterRepo.UpdateMonster(
                id,
                monster
            );
        }
        private void RequireAdmin()
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            int? accountId = session?.GetInt32("AccountID");
            string? role = session?.GetString("Role");

            if (accountId == null ||
                !_accessControl.CanManageMonsters(role))
            {
                throw new UnauthorizedAccessException(
                    "Du har ikke tilladelse til at administrere monstre.");
            }
        }
    }
}