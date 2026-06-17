namespace GameCore;

public class RoomMatch
{
    public int Id { get; }
    public PlayerUser? White { get; set; }
    public PlayerUser? Black { get; set; }
    public List<Watcher> Watchers { get; } = new();
    public bool Started { get; set; }

    public RoomMatch(int id, PlayerUser? white, PlayerUser? black)
    {
        Id = id;
        White = white;
        Black = black;
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
        if(!Watchers.Contains(watch) && Watchers.Count > 0 && watch != null) {            
            Watchers.Add(watch);
            return true;
        }
        return false;
    }
    public void SendAll(INetworkPacket packet)
    {
        foreach (var client in Everyone())
            client.Send(packet);
    }

    public void StartMatch()
    {

    }
}
