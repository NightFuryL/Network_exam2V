using DatabaseLibrary.Core;
using DatabaseLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseLibrary.Core;

public class DataManager : IDataManager
{
    public UserEntity? GetUser(int id)
    {
        using CheckersDbContext db = new();
        return db.Users.FirstOrDefault(x => x.Id == id);
    }

    public UserEntity? GetUser(string name)
    {
        using CheckersDbContext db = new();
        string key = name.Trim().ToLowerInvariant();
        return db.Users.AsEnumerable().FirstOrDefault(x => x.Name.ToLowerInvariant() == key);
    }

    public List<UserEntity> GetTopPlayers(int count)
    {
        using CheckersDbContext db = new();
        return db.Users
            .OrderByDescending(x => x.Rating)
            .Take(count)
            .ToList();
    }

    public void RegisterUser(UserEntity user)
    {
        using CheckersDbContext db = new();
        db.Users.Add(user);
        db.SaveChanges();
    }

    public void UpdateUser(UserEntity user)
    {
        using CheckersDbContext db = new();
        db.Users.Update(user);
        db.SaveChanges();
    }

    public void AddGame(GameHistory game)
    {
        using CheckersDbContext db = new();
        db.Games.Add(game);
        db.SaveChanges();
    }
}
