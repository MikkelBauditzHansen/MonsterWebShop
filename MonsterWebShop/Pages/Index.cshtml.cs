using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Repo;

namespace MonsterWebShop.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IMonsterRepo _monsterRepo;
        public List<Monster> Monsters { get; set; }
        public IndexModel(ILogger<IndexModel> logger, IMonsterRepo monsterRepo)
        {
            _logger = logger;
            _monsterRepo = monsterRepo;
        }

        public void OnGet()
        {
            Monsters = _monsterRepo.GetAllMonsters();
        }
    }
}
