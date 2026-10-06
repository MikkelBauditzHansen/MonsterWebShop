using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Services;
using System.Text.Json;

namespace MonsterWebShop.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MonsterService _monsterService;
        public List<Monster> FeaturedMonsters { get; set; } = new();
        public IndexModel(MonsterService monsterService)
        {
            _monsterService = monsterService;
        }
        public void OnGet()
        {
            FeaturedMonsters = _monsterService
                .SearchAndFilter("", "")
                .Take(3)
                .ToList();
        }
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}