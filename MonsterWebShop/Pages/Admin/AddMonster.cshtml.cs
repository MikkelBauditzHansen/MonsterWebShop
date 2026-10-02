using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonsterWebShop.Models;
using MonsterWebShop.Services;

namespace MonsterWebShop.Pages.Admin
{
    public class AddMonsterModel : PageModel
    {
        private readonly MonsterService _monsterService;

        public AddMonsterModel(MonsterService monsterService)
        {
            _monsterService = monsterService;
        }


        // Fælles oplysninger
        [BindProperty]
        public string? Name { get; set; }

        [BindProperty]
        public string? Color { get; set; }

        [BindProperty]
        public IFormFile? ImagePath { get; set; }

        [BindProperty]
        public int Age { get; set; }

        [BindProperty]
        public double Price { get; set; }


        // Monster type
        [BindProperty]
        public string? MonsterType { get; set; }


        // Dragon
        [BindProperty]
        public double WingSpan { get; set; }

        [BindProperty]
        public DragonType DragonType { get; set; }


        // Humanoid
        [BindProperty]
        public HumanoidType HumanoidType { get; set; }


        // Undead
        [BindProperty]
        public UndeadType UndeadType { get; set; }


        public IActionResult OnGet()
        {
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }


        public IActionResult OnPost()
        {
            // Tjek at brugeren stadig er administrator
            string? role = HttpContext.Session.GetString("Role");

            // Hvis du vil aktivere admin-tjekket igen:
            // if (role != "Admin")
            // {
            //     return RedirectToPage("/Index");
            // }


            string imagePath = "";

            // Tjek om der er uploadet et billede
            if (ImagePath != null)
            {
                // Tilladte filendelser
                string[] allowedExtensions =
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".gif",
                    ".webp"
                };

                // Tilladte filtyper
                string[] allowedContentTypes =
                {
                    "image/jpeg",
                    "image/png",
                    "image/gif",
                    "image/webp"
                };

                string extension = Path.GetExtension(ImagePath.FileName).ToLower();

                // Tjek både filendelse og ContentType
                if (!allowedExtensions.Contains(extension) ||
                    !allowedContentTypes.Contains(ImagePath.ContentType))
                {
                    ModelState.AddModelError(
                        "ImagePath",
                        "Du må kun uploade billeder (JPG, PNG, GIF eller WEBP)."
                    );

                    return Page();
                }


                // Lav et unikt filnavn
                string fileName = Guid.NewGuid().ToString() + extension;

                string folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images"
                );

                Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);

                // Gem billedet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImagePath.CopyTo(stream);
                }

                imagePath = "/images/" + fileName;
            }


            Monster monster;


            // Opret Dragon
            if (MonsterType == "Dragon")
            {
                monster = new Dragon(
                    Name,
                    Color,
                    imagePath,
                    Age,
                    Price,
                    WingSpan,
                    DragonType
                );
            }

            // Opret Humanoid
            else if (MonsterType == "Humanoid")
            {
                monster = new Humanoid(
                    Name,
                    Color,
                    imagePath,
                    Age,
                    Price,
                    HumanoidType
                );
            }

            // Opret Undead
            else if (MonsterType == "Undead")
            {
                monster = new Undead(
                    Name,
                    Color,
                    imagePath,
                    Age,
                    Price,
                    UndeadType
                );
            }

            // Hvis ingen type er valgt
            else
            {
                ModelState.AddModelError(
                    "MonsterType",
                    "Du skal vælge en monstertype."
                );

                return Page();
            }


            // Send monsteret til service
            _monsterService.AddMonster(monster);


            // Gå tilbage til forsiden
            return RedirectToPage("/Index");
        }
    }
}