using MonsterWebShop.Models;

namespace MonsterWebShop.Repo
{
    public interface IAccountRepo
    {
        Account? GetAccount(int accountId);
        Account? GetAccountByUsername(string username);
        void AddAccount(Account account);
        void UpdateAccount(Account account);
        void DeleteAccount(int accountId);
    }
}
