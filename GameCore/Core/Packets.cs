namespace GameCore;
public class NetworkPacket
{
    public PacketType CommandCode { get; set; }
    public PacketWhere PacketTo { get; set; }
    public PacketResult PResult { get; set; } 
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Метод упаковки: превращает весь пакет в один плоский массив байт для отправки в сеть
    /// </summary>
    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write((byte)CommandCode);
        writer.Write((byte)PacketTo);
        writer.Write((byte)PResult);

        // 2. Пишем размер массива Data (чтобы принимающий знал, сколько байт читать)
        if (Data != null)
        {
            writer.Write(Data.Length); // Длина массива (4 байта int)
            writer.Write(Data);        // Сами байты данных
        }
        else
        {
            writer.Write(0); // Если данных нет, пишем длину 0
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Статический метод распаковки: собирает объект NetworkPacket из пришедших байт
    /// </summary>
    public static NetworkPacket Deserialize(byte[] fullPacketBytes)
    {
        using var ms = new MemoryStream(fullPacketBytes);
        using var reader = new BinaryReader(ms);

        var packet = new NetworkPacket();

        // 1. Читаем заголовки в том же порядке, в каком записывали
        packet.CommandCode = (PacketType)reader.ReadByte();
        packet.PacketTo = (PacketWhere)reader.ReadByte();
        packet.PResult = (PacketResult)reader.ReadByte();

        // 2. Читаем размер данных
        int dataLength = reader.ReadInt32();

        if (dataLength > 0)
        {
            packet.Data = reader.ReadBytes(dataLength);
        }
        else
        {
            packet.Data = Array.Empty<byte>();
        }

        return packet;
    }
}


#region old code
/*
public interface INetworkPacket
{
    PacketType CommandCode { get; }
    PacketWhere PacketTo { get; }
    PacketResult PResult { get; }
    string Pack();
    void Unpack(string[] parts);
}
public static class PacketFactory
{
    public static string ToJson<T>(T value) => JsonSerializer.Serialize(value);
    public static T? FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json);
    public static INetworkPacket Parse(string line)
    {
        string[] parts = line.Trim().Split(";;;");
        PacketType type = Enum.Parse<PacketType>(parts[0]);
        INetworkPacket packet = type switch
        {
            PacketType.LoginAndRegister => new RegisterManagePacket(),
            PacketType.LeaderBoardData => new LeaderBoardManagePacket(),
            PacketType.FindGame => new FindGamePacket(),
            PacketType.MatchStartedPacket => new MatchStartedPacket(),
            PacketType.BoardStatePacket => new BoardStatePacket(),
            PacketType.MoveSyncPacket => new MoveSyncPacket(),
            PacketType.RoomsListPacket => new RoomsListManagePacket(),
            PacketType.ManageConnectionPacket => new ManageConnectionPacket(),
            PacketType.GameEndPacket => new GameStatePacket(),
            _ => throw new ArgumentOutOfRangeException()
        };
        packet.Unpack(parts);
        return packet;
    }
}
public class RegisterManagePacket : INetworkPacket
{
    public PacketType CommandCode => PacketType.LoginAndRegister;
    public PacketWhere PacketTo { get; set; }
    public PacketResult PResult { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public UserEntity? responseClientUser { get; set; };
    public string Pack() => $"{CommandCode};;;{PacketTo};;;{PResult};;;{Username};;;{Password};;;{PacketFactory.ToJson(responseClientUser)}";

    public void Unpack(string[] parts)
    {
        Username = parts[1];
        PacketTo = Enum.Parse<PacketWhere>(parts[2]);
        PResult = Enum.Parse<PacketResult>(parts[3]);
        Password = parts[4];
        responseClientUser = PacketFactory.FromJson<UserEntity>(parts[5]);
    }
}
public class LeaderBoardManagePacket : INetworkPacket
{
    public PacketWhere PacketTo { get; set; }
    public PacketResult PResult { get; set; }
    public PacketType CommandCode => PacketType.LeaderBoardData;
    public List<UserEntity> TopUsers { get; set; } = new();
    public string Pack() => $"{CommandCode};;;{PacketTo};;;{PResult};;;{PacketFactory.ToJson(TopUsers)}";
    public void Unpack(string[] parts){
        TopUsers = PacketFactory.FromJson<List<UserEntity>>(parts[1]) ?? new();
        PResult = Enum.Parse<PacketResult>(parts[3]);
        PacketTo = Enum.Parse<PacketWhere>(parts[2]);
    }
}
public class FindGamePacket : INetworkPacket
{
    public PacketType CommandCode => PacketType.FindGame;
    public PacketWhere PacketTo { get; set; }
    public PacketResult PResult { get; }
    public string Pack() => CommandCode.ToString();
    public void Unpack(string[] parts) { }
}
public class MatchStartedPacket : INetworkPacket
{
    public PacketType CommandCode => PacketType.MatchStartedPacket;
    public int RoomId { get; set; }
    public string WhiteName { get; set; } = "";
    public string BlackName { get; set; } = "";
    public PlayerColor? YourColor { get; set; }
    public ClientType Role { get; set; }

    public string Pack() =>
        $"{CommandCode};;;{RoomId};;;{WhiteName};;;{BlackName};;;{YourColor};;;{Role}";

    public void Unpack(string[] parts)
    {
        RoomId = int.Parse(parts[1]);
        WhiteName = parts[2];
        BlackName = parts[3];
        YourColor = string.IsNullOrEmpty(parts[4]) ? null : Enum.Parse<PlayerColor>(parts[4]);
        Role = Enum.Parse<ClientType>(parts[5]);
    }
}
public class BoardStatePacket : INetworkPacket
{
    public PacketType CommandCode => PacketType.BoardStatePacket;
    public BoardState State { get; set; } = new();

    public string Pack() => $"{CommandCode};;;{PacketFactory.ToJson(State)}";

    public void Unpack(string[] parts) =>
        State = PacketFactory.FromJson<BoardState>(parts[1]) ?? new();
}
public class MoveSyncPacket : INetworkPacket
{
    public PacketType CommandCode => PacketType.MoveSyncPacket;
    public int RoomId { get; set; }
    public MoveInfo Move { get; set; } = new();
    public bool Success { get; set; } = true;
    public string Error { get; set; } = "";

    public string Pack() =>
        $"{CommandCode};;;{RoomId};;;{PacketFactory.ToJson(Move)};;;{Success};;;{Error}";

    public void Unpack(string[] parts)
    {
        RoomId = int.Parse(parts[1]);
        Move = PacketFactory.FromJson<MoveInfo>(parts[2]) ?? new();
        Success = parts.Length > 3 && bool.Parse(parts[3]);
        Error = parts.Length > 4 ? parts[4] : "";
    }
}
public class ManageConnectionPacket : INetworkPacket
{
    public PacketType CommandCode => PacketType.ManageConnectionPacket;
    public ConnectionType ConnectionType { get; set; }
    public int RoomId { get; set; }
    public string Pack() => $"{CommandCode};;;{RoomId};;;{ConnectionType}";

    public void Unpack(string[] parts)
    {
        RoomId = int.Parse(parts[1]);
        ConnectionType = Enum.Parse<ConnectionType>(parts[2]);
    }
}
public class GameStatePacket : INetworkPacket
{
    public PacketType CommandCode => PacketType.GameEndPacket;
    public int RoomId { get; set; }
    public GameState Result { get; set; }

    public string Pack() => $"{CommandCode};;;{RoomId};;;{Result}";

    public void Unpack(string[] parts)
    {
        RoomId = int.Parse(parts[1]);
        Result = Enum.Parse<GameState>(parts[2]);
    }
}
public class RoomsListManagePacket : INetworkPacket
{
    public PacketType CommandCode => PacketType.RoomsListPacket;
    public List<RoomMatch> Rooms { get; set; } = new();

    public PacketWhere PacketTo {  get; set; }

    public PacketResult PResult {  get; set; }

    public string Pack() => $"{CommandCode};;;{PacketTo};;;{PResult};;;{PacketFactory.ToJson(Rooms)}";

    public void Unpack(string[] parts) {
        Rooms = PacketFactory.FromJson<List<RoomMatch>>(parts[1]) ?? new();
        PacketTo = Enum.Parse<PacketWhere>(parts[2]);
        PResult = Enum.Parse<PacketResult>(parts[3]);
    }
}
*/
#endregion
