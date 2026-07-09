using DatabaseLibrary.Entities;

namespace DatabaseLibrary.Core;

public interface IDataManager
{
    UserEntity? GetUser(int id);
    UserEntity? GetUser(string name);
    List<UserEntity> GetTopPlayers(int count);
    void AddGame(GameHistory game);
    void UpdateUser(UserEntity user);
    void RegisterUser(UserEntity user);
}
