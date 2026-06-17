namespace GameCore;

public enum ClientType
{
    Watcher,
    Player,
}

public enum PlayerColor
{
    White,
    Black,
}
public enum IsKing
{
    Yes,
    No,
}
public enum ObjectManipulation
{
    Add,
    Remove,
}
public enum CellType
{
    Empty = 0,
    BlackPawn = 1,
    BlackKing = 2,
    WhitePawn = 3,
    WhiteKing = 4,
}

public enum ConnectionType
{
    Exit,
    Connect,
}

public enum GameState
{
    InProgress,
    WhiteWin,
    BlackWin,
    Draw,
}
public enum PacketWhere
{
    ToServer,
    ToClient,
}
public enum PacketResult
{
    Success,
    Failed,
}
public enum PacketType : byte
{
    ResultOfDoPacket,
    LoginAndRegister,
    LeaderBoardData,
    FindGame,
    CancelFindGame,
    MatchStartedPacket,
    BoardStatePacket,
    MoveSyncPacket,
    RoomsListForWatchersPacket,
    ManageConnectionPacket,
    GameEndPacket,
}
