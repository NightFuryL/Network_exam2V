namespace GameCore;
public class PlayerUser
{
    public int UserId { get; set; }
    public string Name { get; set; } = "Guest";
    public int Raiting { get; set; }
    public ClientType Role { get; set; } = ClientType.Player;
    public PlayerColor? Color { get; set; }
    public ClientConnection ClientConnection { get; set; }
}