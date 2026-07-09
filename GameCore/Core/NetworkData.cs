using DatabaseLibrary.Entities;

namespace GameCore;

public class MatchStartedData
{
    public int RoomId { get; set; }
    public string WhiteName { get; set; } = "";
    public string BlackName { get; set; } = "";
    public PlayerColor? YourColor { get; set; }
    public ClientType Role { get; set; }
    public BoardState? InitialBoard { get; set; }
}

public class RoomInfoData
{
    public int Id { get; set; }
    public string WhiteName { get; set; } = "";
    public string BlackName { get; set; } = "";
    public bool Started { get; set; }
}

public class MoveSyncData
{
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public MoveInfo Move { get; set; } = new();
    public bool Success { get; set; } = true;
    public string Error { get; set; } = "";
}

public class ManageConnectionData
{
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public ConnectionType ConnectionType { get; set; }
}

public class FindGameData
{
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
}

public class GameEndData
{
    public int RoomId { get; set; }
    public GameState Result { get; set; }
    public bool ByForfeit { get; set; }
    public UserEntity? WhiteUser { get; set; }
    public UserEntity? BlackUser { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
}
