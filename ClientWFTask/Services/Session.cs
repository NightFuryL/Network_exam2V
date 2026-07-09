using DatabaseLibrary.Entities;
using GameCore;

namespace ClientWFTask.Services;

public static class Session
{
    public static int Id { get; private set; }
    public static string Name { get; private set; } = "";
    public static int Points { get; private set; }
    public static PlayerColor? Color { get; set; }
    public static ClientType Role { get; set; }
    public static int RoomId { get; set; }

    public static void SetUser(UserEntity user)
    {
        Id = user.Id;
        Name = user.Name;
        Points = user.Rating;
    }

    public static void UpdateRating(int rating)
    {
        Points = rating;
    }

    public static void Clear()
    {
        Id = 0;
        Name = "";
        Points = 0;
        Color = null;
        Role = ClientType.Player;
        RoomId = 0;
    }
}
