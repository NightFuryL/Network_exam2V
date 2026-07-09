namespace GameCore;
public class Watcher
{
    public ClientType ClientType { get; set; } = ClientType.Watcher;
    public ClientConnection clientConnection = null!;
}
