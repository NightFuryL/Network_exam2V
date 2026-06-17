using DatabaseLibrary.Core;
using DatabaseLibrary.Entities;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
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

        // Создаем буфер для заголовка ОДИН РАЗ перед циклом (1 + 1 + 1 + 4 = 7 байт)
        byte[] headerBuffer = new byte[7];

        try
        {
            while (true)
            {
                // 1. Читаем строго 7 байт заголовка. 
                // Если клиент отключился, метод выбросит EndOfStreamException, и мы уйдем в catch/finally.
                await stream.ReadExactlyAsync(headerBuffer, 0, headerBuffer.Length);

                // Достаем длину массива Data из последних 4 байт заголовка (начиная с 3-го индекса)
                int dataLength = BitConverter.ToInt32(headerBuffer, 3);

                // 2. Создаем итоговый массив под ВЕСЬ пакет (7 байт заголовка + длина данных)
                byte[] fullPacketBytes = new byte[7 + dataLength];

                // Копируем уже прочитанный заголовок в начало итогового массива
                Array.Copy(headerBuffer, 0, fullPacketBytes, 0, 7);

                if (dataLength > 0)
                {
                    // Дочитываем из сети оставшиеся байты данных прямо в хвост нашего массива (с 7-го индекса)
                    await stream.ReadExactlyAsync(fullPacketBytes, 7, dataLength);
                }

                // 3. Передаем ГАРАНТИРОВАННО целый и чистый массив байт в твой метод десериализации
                NetworkPacket packet = NetworkPacket.Deserialize(fullPacketBytes);

                Console.WriteLine($"New packet from {client.Client.RemoteEndPoint}: {packet.CommandCode}");

                // Передаем пакет дальше в твою логику обработки
                await DoBySwitchPacketAndSend(packet, client);
            }
        }
        catch (EndOfStreamException)
        {
            // Это нормальное поведение, когда клиент просто закрыл приложение или отключился
            Console.WriteLine($"Client disconnected normally: {client.Client.RemoteEndPoint}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            progress?.Report($"ERROR: {ex.ToString()}");
        }
        finally
        {
            stream.Close();
            client.Close();
        }
    }

    public static async Task DoBySwitchPacketAndSend(NetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket == null) return;
        switch (networkPacket.CommandCode)
        {
            case PacketType.LoginAndRegister: await DoLoginRegistration(networkPacket, client); break;
            case PacketType.LeaderBoardData: await DoLeaderBoard(networkPacket, client); break;
            case PacketType.FindGame: await DoFindGame(networkPacket, client); break;
            case PacketType.MatchStartedPacket: await DoMatchStarted(networkPacket, client); break;
            case PacketType.BoardStatePacket: await DoBoardState(networkPacket, client); break;
            case PacketType.MoveSyncPacket: await DoMoveSync(networkPacket, client); break;
            case PacketType.RoomsListForWatchersPacket: await DoRoomListForWathcers(networkPacket, client); break;
            case PacketType.ManageConnectionPacket: await DoManageConnection(networkPacket, client); break;
            case PacketType.GameEndPacket: await DoGameState(networkPacket, client); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
    private static async Task DoGameState(NetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoManageConnection(NetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoMoveSync(NetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoBoardState(NetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoMatchStarted(NetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoFindGame(NetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
    }
    private static async Task DoRoomListForWathcers(NetworkPacket networkPacket, TcpClient client)
    {
        NetworkPacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.RoomsListForWatchersPacket) { throw new NotImplementedException(); }
            var packet = new NetworkPacket();
            responsePacket = packet;
            responsePacket.PacketTo = PacketWhere.ToClient;
            responsePacket.Data = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_rooms));
            responsePacket.PResult = PacketResult.Success;
        }
        catch (Exception)
        {
            responsePacket = new NetworkPacket()
            {
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        byte[] response = responsePacket.Serialize();
        client?.GetStream().WriteAsync(response);
    }
    private static async Task DoLeaderBoard(NetworkPacket networkPacket, TcpClient client)
    {
        NetworkPacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.LeaderBoardData) { throw new NotImplementedException(); }
            var packet = networkPacket;
            if (packet.PacketTo != PacketWhere.ToClient) { throw new NotImplementedException(); }
            //Чекаємо що отримане будет код пакета - LeaderBoardData, щоб не забивати пакет ще дата 
            responsePacket = packet;
            //Передаємо одразу в Data серіалізований список топ 20 гравців з БД
            responsePacket.Data = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_db?.GetTopPlayers(20)));
            responsePacket.PacketTo = PacketWhere.ToClient;
            responsePacket.PResult = PacketResult.Success;
        }
        catch (Exception ex)
        {
            responsePacket = new NetworkPacket()
            {
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        byte[] response = responsePacket.Serialize();
        await client.GetStream().WriteAsync(response);
    }
    private static async Task DoLoginRegistration(NetworkPacket networkPacket, TcpClient client)
    {
        NetworkPacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.LoginAndRegister) { throw new NotImplementedException(); }
            var packet = networkPacket;
            var dictData = new Dictionary<string, string>();
            /*Дані які приймаємо як ключ - як значення
             * Username - string ім'я користувача після логіну або реєстрації
             * Password - string пароль користувача після логіну або реєстрації
             * 
             */
            UserEntity newUser = new UserEntity()
            {
                Name = dictData["Username"],
                Password = dictData["Password"],
                Rating = 100,
                Wins = 0,
                Loses = 0,
                Draws = 0,
            };
            _db.RegisterUser(newUser);
            responsePacket = new NetworkPacket();
            /*Дані які відправляємо як ключ - як значення
             * Все те що отримали від клієнта + додатково
             * ResponseClientUser - UserEntity з усіма полями, включаючи Id, який присвоївся при реєстрації в БД
             */
            responsePacket.PResult = PacketResult.Success;
            responsePacket.PacketTo = PacketWhere.ToClient;
            dictData.Add("ResponseClientUser", JsonSerializer.Serialize<UserEntity>(newUser));
            responsePacket.Data = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dictData));
        }
        catch (Exception ex)
        {
            responsePacket = new NetworkPacket()
            {
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        byte[] response = responsePacket.Serialize();
        await client.GetStream().WriteAsync(response);
    }
    public static void Stop()
    {
        _running = false;
        try { _serverListener?.Stop(); } catch { }
    }
}