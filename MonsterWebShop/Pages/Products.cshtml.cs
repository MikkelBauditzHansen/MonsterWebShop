using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Services;
using System.Text.Json;

namespace MonsterWebShop.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly MonsterService _monsterService;

        public ProductsModel(MonsterService monsterService)
        {
            _monsterService = monsterService;
        }

        public List<Monster> Monsters { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchText { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string MonsterType { get; set; } = "";

        public int CartCount { get; set; }

        public void OnGet()
        {
            Monsters =
                _monsterService.SearchAndFilter(
                    SearchText,
                    MonsterType
                );

            string? cartJson =
                HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                CartCount = 0;
                return;
            }

            try
            {
                List<int> cart =
                    JsonSerializer.Deserialize<List<int>>(cartJson)
                    ?? new List<int>();

                CartCount = cart.Count;
            }
            catch
            {
                HttpContext.Session.Remove("Cart");
                CartCount = 0;
            }
        }
        
        public IActionResult OnPostAddToCart(
            int monsterId)
        {
            string? cartJson =
                HttpContext.Session.GetString("Cart");

            List<int> cart;

            if (string.IsNullOrEmpty(cartJson))
            {
                cart = new List<int>();
            }
            else
            {
                try
                {
                    cart =
                        JsonSerializer.Deserialize<List<int>>(cartJson)
                        ?? new List<int>();
                }
                catch
                {
                    cart = new List<int>();
                }
            }

            cart.Add(monsterId);

            HttpContext.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cart)
            );

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(
            int monsterId)
        {
            string? role =
                HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToPage();
            }

            _monsterService.RemoveMonster(monsterId);

            return RedirectToPage();
        }

        public IActionResult OnPostEdit(
            int monsterId,
            string name,
            string color,
            int age,
            double price,
            double? wingSpan,
            int? monsterSubtype,
            IFormFile? newImage)
        {
            // Kun administratorer må redigere
            string? role = HttpContext.Session.GetString("Role");
            int? accountId = HttpContext.Session.GetInt32("AccountID");

            if (accountId == null || role != "Admin")
            {
                return Forbid();
            }

            // Hent monsteret fra databasen
            Monster? monster =
                _monsterService.GetMonsterById(monsterId);

            if (monster == null)
            {
                return NotFound();
            }

            // Valider almindelige oplysninger
            if (string.IsNullOrWhiteSpace(name) ||
                name.Length > 100 ||
                string.IsNullOrWhiteSpace(color) ||
                color.Length > 50 ||
                age < 0 ||
                !double.IsFinite(price) ||
                price < 0)
            {
                return BadRequest("Ugyldige monsteroplysninger.");
            }

            // Opdater fælles oplysninger
            monster.Name = name.Trim();
            monster.Color = color.Trim();
            monster.Age = age;
            monster.Price = price;

            // Opdater oplysninger for monstertypen
            if (monster is Dragon dragon)
            {
                if (wingSpan == null ||
                    !double.IsFinite(wingSpan.Value) ||
                    wingSpan.Value < 0 ||
                    monsterSubtype == null ||
                    !Enum.IsDefined(
                        typeof(DragonType),
                        monsterSubtype.Value))
                {
                    return BadRequest("Ugyldige dragon-oplysninger.");
                }

                dragon.WingSpan = wingSpan.Value;
                dragon.Type = (DragonType)monsterSubtype.Value;
            }
            else if (monster is Humanoid humanoid)
            {
                if (monsterSubtype == null ||
                    !Enum.IsDefined(
                        typeof(HumanoidType),
                        monsterSubtype.Value))
                {
                    return BadRequest("Ugyldig humanoid-type.");
                }

                humanoid.Type = (HumanoidType)monsterSubtype.Value;
            }
            else if (monster is Undead undead)
            {
                if (monsterSubtype == null ||
                    !Enum.IsDefined(
                        typeof(UndeadType),
                        monsterSubtype.Value))
                {
                    return BadRequest("Ugyldig undead-type.");
                }

                undead.Type = (UndeadType)monsterSubtype.Value;
            }

            if (newImage != null && newImage.Length > 0)
            {
                // Maksimal filstørrelse: 5 MB
                if (newImage.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Billedet må højst fylde 5 MB.");
                }

                // Kontrollér filtypen
                string extension =
                    Path.GetExtension(newImage.FileName).ToLowerInvariant();

                string[] allowedExtensions =
                {
        ".jpg", ".jpeg", ".png", ".webp"
    };

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Kun JPG, PNG og WebP er tilladt.");
                }

                // Kontrollér filens faktiske format via signatur
                byte[] header = new byte[12];

                using (Stream stream = newImage.OpenReadStream())
                {
                    int bytesRead = stream.Read(header, 0, header.Length);

                    bool isJpeg =
                        bytesRead >= 3 &&
                        header[0] == 0xFF &&
                        header[1] == 0xD8 &&
                        header[2] == 0xFF;

                    bool isPng =
                        bytesRead >= 8 &&
                        header.AsSpan(0, 8).SequenceEqual(
                            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

                    bool isWebp =
                        bytesRead >= 12 &&
                        System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
                        System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";

                    bool validFormat = extension switch
                    {
                        ".jpg" or ".jpeg" => isJpeg,
                        ".png" => isPng,
                        ".webp" => isWebp,
                        _ => false
                    };

                    if (!validFormat)
                    {
                        return BadRequest("Billedfilens format er ugyldigt.");
                    }
                }

                // Opret uploadmappen
                string uploadFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "monsters"
                );

                Directory.CreateDirectory(uploadFolder);

                // Generér et unikt filnavn
                string fileName =
                    Guid.NewGuid().ToString("N") + extension;

                string filePath =
                    Path.Combine(uploadFolder, fileName);

                // Gem billedet
                using (FileStream fileStream =
                       new FileStream(filePath, FileMode.CreateNew))
                {
                    newImage.CopyTo(fileStream);
                }

                // Opdater billedstien på monsteret
                monster.ImagePath = "/uploads/monsters/" + fileName;
            }


            // Gem ændringerne i databasen
            _monsterService.UpdateMonster(monsterId, monster);

            return RedirectToPage();
        }

    }
}