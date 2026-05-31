namespace DatabaseLibrary.Entities;
public class GameHistory
{
    public int Id { get; set; }

    public int WhiteId { get; set; }

    public int BlackId { get; set; }

    public int WinnerId { get; set; }

    public string Moves { get; set; } = "";

    public DateTime Date { get; set; }
}