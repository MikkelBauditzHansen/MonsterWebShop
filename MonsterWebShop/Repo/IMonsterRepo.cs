using MonsterWebShop.Models;

namespace MonsterWebShop.Repo
{
    public interface IMonsterRepo
    {
        Monster AddMonster(Monster monster);
        List<Monster> GetAllMonsters();
        Monster? GetMonsterById(int id);
        Monster? RemoveMonster(int id);
        Monster? UpdateMonster(int id, Monster updatedMonster);
    }
}