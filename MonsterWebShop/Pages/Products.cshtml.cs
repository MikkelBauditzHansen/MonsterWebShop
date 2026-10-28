
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
        private readonly CartCookieService _cartCookieService;

        private readonly AccessControlService _accessControl;
        public ProductsModel(MonsterService monsterService, AccessControlService accessControlService)
        {
            _monsterService = monsterService;
            _accessControl = accessControlService;
        }

        public List<Monster> Monsters { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchText { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string MonsterType { get; set; } = "";

        public int CartCount { get; set; }

        public ProductsModel(
            MonsterService monsterService,
            CartCookieService cartCookieService)
        {
            _monsterService = monsterService;
            _cartCookieService = cartCookieService;
        }

        // Vis produkter og antal i kurven
        public void OnGet()
        {
            Monsters =
                _monsterService.SearchAndFilter(
                    SearchText,
                    MonsterType
                );

            List<int> cart = GetCart();

            CartCount = cart.Count;
        }

        // Tilføj monster til kurven
        public IActionResult OnPostAddToCart(int monsterId)
        {
            // Kontroller at monsteret findes
            Monster? monster =
                _monsterService.GetMonsterById(monsterId);

            if (monster == null)
            {
                return NotFound();
            }

            // Hent kurven fra Session eller cookie
            List<int> cart = GetCart();

            // Tilføj monsteret
            cart.Add(monsterId);

            // Gem både i Session og cookie
            SaveCart(cart);

            return RedirectToPage();
        }

        // Hent kurven fra Session eller cookie
        private List<int> GetCart()
        {
            string? cartJson =
                HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                try
                {
                    List<int>? cart =
                        JsonSerializer.Deserialize<List<int>>(cartJson);

                    return cart ?? new List<int>();
                }
                catch
                {
                    // Hvis Session er ugyldig,
                    // forsøger vi at hente fra cookie
                }
            }

            // Hent den gemte kurv fra cookie
            List<int> cookieCart =
                _cartCookieService.GetCart(HttpContext);

            // Gendan kurven i Session
            HttpContext.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cookieCart)
            );

            return cookieCart;
        }

        // Gem kurven i både Session og cookie
        private void SaveCart(List<int> cart)
        {
            // Gem i Session
            HttpContext.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cart)
            );

            // Gem i cookie i 30 dage
            _cartCookieService.SaveCart(
                HttpContext,
                cart
            );
        }

        // Slet monster fra webshoppen
        public IActionResult OnPostDelete(int monsterId)
        {
            int? accountId = HttpContext.Session.GetInt32("AccountID");
            string? role = HttpContext.Session.GetString("Role");

            if (accountId == null ||
                !_accessControl.CanManageMonsters(role))
            {
                return StatusCode(403);
            }

            if (role != "Admin")
            {
                return RedirectToPage();
            }

            _monsterService.RemoveMonster(monsterId);

            return RedirectToPage();
        }

        // Rediger monster
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
            int? accountId = HttpContext.Session.GetInt32("AccountID");
            string? role = HttpContext.Session.GetString("Role");

            if (accountId == null ||
                !_accessControl.CanManageMonsters(role))
            {
                return StatusCode(403);
            }

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
                return BadRequest(
                    "Ugyldige monsteroplysninger.");
            }

            // Opdater fælles oplysninger
            monster.Name = name.Trim();
            monster.Color = color.Trim();
            monster.Age = age;
            monster.Price = price;

            // Opdater oplysninger for Dragon
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
                    return BadRequest(
                        "Ugyldige dragon-oplysninger.");
                }

                dragon.WingSpan = wingSpan.Value;
                dragon.Type =
                    (DragonType)monsterSubtype.Value;
            }

            // Opdater oplysninger for Humanoid
            else if (monster is Humanoid humanoid)
            {
                if (monsterSubtype == null ||
                    !Enum.IsDefined(
                        typeof(HumanoidType),
                        monsterSubtype.Value))
                {
                    return BadRequest(
                        "Ugyldig humanoid-type.");
                }

                humanoid.Type =
                    (HumanoidType)monsterSubtype.Value;
            }

            // Opdater oplysninger for Undead
            else if (monster is Undead undead)
            {
                if (monsterSubtype == null ||
                    !Enum.IsDefined(
                        typeof(UndeadType),
                        monsterSubtype.Value))
                {
                    return BadRequest(
                        "Ugyldig undead-type.");
                }

                undead.Type =
                    (UndeadType)monsterSubtype.Value;
            }

            // Upload nyt billede
            if (newImage != null && newImage.Length > 0)
            {
                // Maksimal filstørrelse: 5 MB
                if (newImage.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(
                        "Billedet må højst fylde 5 MB.");
                }

                // Kontrollér filtypen
                string extension =
                    Path.GetExtension(
                        newImage.FileName
                    ).ToLowerInvariant();

                string[] allowedExtensions =
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(
                        "Kun JPG, PNG og WebP er tilladt.");
                }

                // Kontrollér billedets faktiske format
                byte[] header = new byte[12];

                using (Stream stream =
                       newImage.OpenReadStream())
                {
                    int bytesRead =
                        stream.Read(
                            header,
                            0,
                            header.Length
                        );

                    bool isJpeg =
                        bytesRead >= 3 &&
                        header[0] == 0xFF &&
                        header[1] == 0xD8 &&
                        header[2] == 0xFF;

                    bool isPng =
                        bytesRead >= 8 &&
                        header.AsSpan(0, 8).SequenceEqual(
                            new byte[]
                            {
                                137, 80, 78, 71,
                                13, 10, 26, 10
                            });

                    bool isWebp =
                        bytesRead >= 12 &&
                        System.Text.Encoding.ASCII
                            .GetString(header, 0, 4) == "RIFF" &&
                        System.Text.Encoding.ASCII
                            .GetString(header, 8, 4) == "WEBP";

                    bool validFormat = extension switch
                    {
                        ".jpg" or ".jpeg" => isJpeg,
                        ".png" => isPng,
                        ".webp" => isWebp,
                        _ => false
                    };

                    if (!validFormat)
                    {
                        return BadRequest(
                            "Billedfilens format er ugyldigt.");
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
                    Guid.NewGuid().ToString("N") +
                    extension;

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName
                    );

                // Gem billedet
                using (FileStream fileStream =
                       new FileStream(
                           filePath,
                           FileMode.CreateNew))
                {
                    newImage.CopyTo(fileStream);
                }

                // Opdater billedstien
                monster.ImagePath =
                    "/uploads/monsters/" + fileName;
            }

            // Gem ændringerne i databasen
            _monsterService.UpdateMonster(
                monsterId,
                monster
            );

            return RedirectToPage();
        }
    }
}
