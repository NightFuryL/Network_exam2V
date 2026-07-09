namespace GameCore;

public class RoomMatch
{
    public int Id { get; }
    public PlayerUser? White { get; set; }
    public PlayerUser? Black { get; set; }
    public List<Watcher> Watchers { get; } = new();
    public bool Started { get; set; }
    public BoardState Board { get; set; }
    public List<string> MoveHistory { get; } = new();

    public RoomMatch(int id, PlayerUser? white, PlayerUser? black)
    {
        Id = id;
        White = white;
        Black = black;
        Board = BoardState.CreateInitial(id);
    }

    public bool IsFull => White != null && Black != null;
    public int ReturnCountOfWatchers() => Watchers.Count;

    public IEnumerable<ClientConnection> Everyone()
    {
        if (White != null) yield return White.ClientConnection;
        if (Black != null) yield return Black.ClientConnection;
        foreach (var watcher in Watchers) yield return watcher.clientConnection;
    }

    public bool AddWatcher(Watcher watch)
    {
        if (watch != null && !Watchers.Contains(watch))
        {
            Watchers.Add(watch);
            return true;
        }
        return false;
    }

    public async Task SendAllAsync(NetworkPacket packet)
    {
        foreach (var client in Everyone().ToList())
        {
            try
            {
                await client.SendAsync(packet);
            }
            catch
            {
                // A leaving player may already have a closed socket; keep notifying everyone else.
            }
        }
    }

    public PlayerUser? GetPlayer(PlayerColor color) =>
        color == PlayerColor.White ? White : Black;

    public PlayerColor? GetColor(PlayerUser player)
    {
        if (White == player) return PlayerColor.White;
        if (Black == player) return PlayerColor.Black;
        return null;
    }

    public void StartMatch()
    {
        Started = true;
        Board = BoardState.CreateInitial(Id);
    }

    public RoomInfoData ToInfo() => new()
    {
        Id = Id,
        WhiteName = White?.Name ?? "-",
        BlackName = Black?.Name ?? "-",
        Started = Started
    };
}
