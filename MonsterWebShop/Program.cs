using MonsterWebShop.Repo;
using MonsterWebShop.Services;

namespace MonsterWebShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            

            builder.Services.AddRazorPages();
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddScoped<IMonsterRepo> (provider => new MonsterRepoDB(connectionString));
            builder.Services.AddScoped<MonsterService>();
            builder.Services.AddScoped<IAccountRepo>(provider => new AccountRepo(connectionString));
            builder.Services.AddScoped<AccountService>();
            builder.Services.AddScoped<PasswordHasher>();
            builder.Services.AddScoped<PasswordPolicy>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddSession();


            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}