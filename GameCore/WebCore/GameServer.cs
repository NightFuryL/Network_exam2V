using DatabaseLibrary.Core;
using DatabaseLibrary.Entities;
using System.Net;
using System.Net.Sockets;
namespace GameCore;

public static class GameServer
{
    private static TcpListener? _serverListener;
    private static IDataManager? _db;
    private static List<RoomMatch> _rooms = new();
    private static Queue<ClientConnection> _searchQueue = new();
    private static int _nextRoomId = 1;
    private static bool _running;
    public static async Task Start(IDataManager db)
    {
        if (_running) return;
        _db = db;
        _serverListener = new TcpListener(IPAddress.Parse(AppSettings.Host), AppSettings.Port);
        _serverListener.Start();
        _running = true;
        while (true)
        {
            TcpClient clientListener = await _serverListener.AcceptTcpClientAsync();
            Console.WriteLine($"New client connected: {clientListener.Client.RemoteEndPoint} - {clientListener.Client.LocalEndPoint}");
            _ = Task.Run(async () => HandleClientAsync(clientListener));
        }
    }
    public static async Task HandleClientAsync(TcpClient client, IProgress<string>? progress = null)
    {
        NetworkStream stream = client.GetStream();
        try
        {
            while (true)
            {
                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                string line = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                INetworkPacket packet = PacketFactory.Parse(line);
                Console.WriteLine($"New packet from {client.Client.RemoteEndPoint}: {packet.CommandCode}");
                await DoBySwitchPacketAndSend(packet, client);

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            if (progress != null) progress?.Report($"ERROR: {ex.ToString()}");
        }
        finally
        {
            stream.Close();
            client.Close();
        }
    }
    public static async Task DoBySwitchPacketAndSend(INetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket == null) return;
        switch (networkPacket)
        {
            case RegisterManagePacket lgp: await DoLoginRegistration(networkPacket, client); break;
            case LeaderBoardManagePacket lbp: await DoLeaderBoard(networkPacket, client); break;
            //case FindGamePacket fgp: await DoFindGame(networkPacket, client); break;
            //case MatchStartedPacket msp: await DoMatchStarted(networkPacket, client); break;
            //case BoardStatePacket bsp: await DoBoardState(networkPacket, client); break;
            //case MoveSyncPacket msp: await DoMoveSync(networkPacket, client); break;
            case RoomsListManagePacket rlp: await DoRoomList(networkPacket, client); break;
            //case ManageConnectionPacket mcp: await DoManageConnection(networkPacket, client); break;
            //case GameStatePacket gep: await DoGameState(networkPacket, client); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private static async Task DoGameState(INetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }

    private static async Task DoManageConnection(INetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }

    private static async Task DoRoomList(INetworkPacket networkPacket, TcpClient client)
    {
        RoomsListManagePacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
            var packet = (RoomsListManagePacket)networkPacket;
            responsePacket = packet;
            responsePacket.PacketTo = PacketWhere.ToClient;
            responsePacket.Rooms = _rooms;
            responsePacket.PResult = PacketResult.Success;
        }
        catch (Exception)
        {
            responsePacket = new RoomsListManagePacket()
            {
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        byte[] response = System.Text.Encoding.UTF8.GetBytes(responsePacket.Pack());
        client?.GetStream().WriteAsync(response);
    }

    private static async Task DoMoveSync(INetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoBoardState(INetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoMatchStarted(INetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoFindGame(INetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }

    private static async Task DoLeaderBoard(INetworkPacket networkPacket, TcpClient client)
    {
        LeaderBoardManagePacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
            var packet = (LeaderBoardManagePacket)networkPacket;
            if (packet.PacketTo != PacketWhere.ToClient) { throw new NotImplementedException(); }
            responsePacket = packet;
            responsePacket.PacketTo = PacketWhere.ToClient;
            responsePacket.TopUsers = _db.GetTopPlayers(20);
            responsePacket.PResult = PacketResult.Success;
        }
        catch (Exception ex)
        {
            responsePacket = new LeaderBoardManagePacket()
            {
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        byte[] response = System.Text.Encoding.UTF8.GetBytes(responsePacket.Pack());
        client?.GetStream().WriteAsync(response);
    }
    private static async Task DoLoginRegistration(INetworkPacket networkPacket, TcpClient client)
    {
        RegisterManagePacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
            var packet = (RegisterManagePacket)networkPacket;
            UserEntity newUser = new UserEntity()
            {
                Name = packet.Username,
                Password = packet.Password, 
                Rating = 100,
                Wins = 0,
                Loses = 0,
                Draws = 0,
            };
            _db.RegisterUser(newUser);
            responsePacket = packet;
            responsePacket.PacketTo = PacketWhere.ToClient;
            responsePacket.responseClientUser = newUser;
            responsePacket.PResult = PacketResult.Success;
        }
        catch (Exception ex)
        {
            responsePacket = new RegisterManagePacket()
            {
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        byte[] response = System.Text.Encoding.UTF8.GetBytes(responsePacket.Pack());
        client?.GetStream().WriteAsync(response);
    }

    public static void Stop()
    {
        _running = false;
        try { _serverListener?.Stop(); } catch { }
    }
}