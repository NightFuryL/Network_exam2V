using DatabaseLibrary.Core;
using DatabaseLibrary.Entities;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
namespace GameCore;

public static class GameServer
{
    private static TcpListener? _serverListener;
    private static IDataManager? _db;
    private static List<RoomMatch> _rooms = new();
    private static readonly Queue<(ClientConnection Conn, PlayerUser Player)> _searchQueue = new();
    private static int _nextRoomId = 1;
    private static bool _running;
    private static readonly ConcurrentDictionary<TcpClient, ClientContext> _clients = new();
    private static readonly ConcurrentDictionary<string, TcpClient> _onlineNames = new();

    private class ClientContext
    {
        public ClientConnection Connection { get; set; } = null!;
        public UserEntity? User { get; set; }
        public PlayerUser? Player { get; set; }
        public int? RoomId { get; set; }
        public string? OnlineName { get; set; }
    }

    private static string NormalizeName(string name) => name.Trim().ToLowerInvariant();

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
            var conn = new ClientConnection(clientListener);
            _clients[clientListener] = new ClientContext { Connection = conn };
            _ = Task.Run(async () => HandleClientAsync(clientListener));
        }
    }

    public static async Task HandleClientAsync(TcpClient client, IProgress<string>? progress = null)
    {
        NetworkStream stream = client.GetStream();
        byte[] headerBuffer = new byte[7];

        try
        {
            while (true)
            {
                await stream.ReadExactlyAsync(headerBuffer, 0, headerBuffer.Length);
                int dataLength = BitConverter.ToInt32(headerBuffer, 3);
                byte[] fullPacketBytes = new byte[7 + dataLength];
                Array.Copy(headerBuffer, 0, fullPacketBytes, 0, 7);
                if (dataLength > 0)
                    await stream.ReadExactlyAsync(fullPacketBytes, 7, dataLength);

                NetworkPacket packet = NetworkPacket.Deserialize(fullPacketBytes);
                Console.WriteLine($"New packet from {client.Client.RemoteEndPoint}: {packet.CommandCode}");
                await DoBySwitchPacketAndSend(packet, client);
            }
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine($"Client disconnected normally: {client.Client.RemoteEndPoint}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            progress?.Report($"ERROR: {ex.ToString()}");
        }
        finally
        {
            CleanupClient(client);
            stream.Close();
            client.Close();
        }
    }

    private static void CleanupClient(TcpClient client)
    {
        if (!_clients.TryRemove(client, out var ctx)) return;

        if (ctx.OnlineName != null)
            _onlineNames.TryRemove(NormalizeName(ctx.OnlineName), out _);

        if (ctx.Player != null)
        {
            var queueList = _searchQueue.ToList();
            _searchQueue.Clear();
            foreach (var item in queueList)
                if (item.Conn.Tcp != client)
                    _searchQueue.Enqueue(item);
        }

        _ = HandleRoomLeaveAsync(client, ctx);
    }

    private static async Task HandleRoomLeaveAsync(TcpClient client, ClientContext ctx)
    {
        if (!ctx.RoomId.HasValue) return;
        var room = _rooms.FirstOrDefault(r => r.Id == ctx.RoomId.Value);
        if (room == null) return;

        PlayerUser? leaver = null;
        if (room.White?.ClientConnection.Tcp == client) leaver = room.White;
        else if (room.Black?.ClientConnection.Tcp == client) leaver = room.Black;

        room.Watchers.RemoveAll(w => w.clientConnection.Tcp == client);

        if (leaver != null && room.Started && room.Board.Result == GameState.InProgress)
        {
            // Якщо гравець звалив з кімнати, не чекаємо нічого ще:
            // другий одразу отримує перемогу, а кімната закривається
            if (room.White?.ClientConnection.Tcp == client) room.White = null;
            if (room.Black?.ClientConnection.Tcp == client) room.Black = null;
            ctx.RoomId = null;
            await ForfeitAndFinish(room, leaver);
            return;
        }

        if (room.White?.ClientConnection.Tcp == client) room.White = null;
        if (room.Black?.ClientConnection.Tcp == client) room.Black = null;
        ctx.RoomId = null;
        if (room.White == null && room.Black == null)
            _rooms.Remove(room);
    }

    public static async Task DoBySwitchPacketAndSend(NetworkPacket networkPacket, TcpClient client)
    {
        if (networkPacket == null) return;
        switch (networkPacket.CommandCode)
        {
            case PacketType.LoginAndRegister: await DoLoginRegistration(networkPacket, client); break;
            case PacketType.LeaderBoardData: await DoLeaderBoard(networkPacket, client); break;
            case PacketType.FindGame: await DoFindGame(networkPacket, client); break;
            case PacketType.CancelFindGame: await DoCancelFindGame(client); break;
            case PacketType.MoveSyncPacket: await DoMoveSync(networkPacket, client); break;
            case PacketType.RoomsListForWatchersPacket: await DoRoomListForWathcers(networkPacket, client); break;
            case PacketType.ManageConnectionPacket: await DoManageConnection(networkPacket, client); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private static ClientContext GetContext(TcpClient client) =>
        _clients.TryGetValue(client, out var ctx) ? ctx : throw new InvalidOperationException("Unknown client");

    private static async Task SendResponse(TcpClient client, NetworkPacket responsePacket)
    {
        var ctx = GetContext(client);
        await ctx.Connection.SendAsync(responsePacket);
    }

    private static async Task DoLoginRegistration(NetworkPacket networkPacket, TcpClient client)
    {
        NetworkPacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.LoginAndRegister) throw new NotImplementedException();
            var req = JsonSerializer.Deserialize<LoginRequest>(networkPacket.Data)
                      ?? throw new Exception("Invalid data");
            string username = req.Username.Trim();
            if (string.IsNullOrWhiteSpace(username)) throw new Exception("Empty nickname");

            string key = NormalizeName(username);
            if (_onlineNames.TryGetValue(key, out var existing) && existing != client)
                throw new Exception("Nickname already online");

            UserEntity? user = _db?.GetUser(username);
            if (user == null)
            {
                user = new UserEntity
                {
                    Name = username,
                    Password = "",
                    Rating = 100,
                    Wins = 0,
                    Loses = 0,
                    Draws = 0,
                };
                _db?.RegisterUser(user);
            }

            var ctx = GetContext(client);
            if (ctx.OnlineName != null)
                _onlineNames.TryRemove(NormalizeName(ctx.OnlineName), out _);

            ctx.User = user;
            ctx.OnlineName = username;
            _onlineNames[key] = client;

            responsePacket = new NetworkPacket
            {
                CommandCode = PacketType.LoginAndRegister,
                PResult = PacketResult.Success,
                PacketTo = PacketWhere.ToClient,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user))
            };
        }
        catch (Exception)
        {
            responsePacket = new NetworkPacket
            {
                CommandCode = PacketType.LoginAndRegister,
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        await SendResponse(client, responsePacket);
    }

    private static async Task DoLeaderBoard(NetworkPacket networkPacket, TcpClient client)
    {
        NetworkPacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.LeaderBoardData) throw new NotImplementedException();
            if (networkPacket.PacketTo != PacketWhere.ToServer) throw new NotImplementedException();
            responsePacket = new NetworkPacket
            {
                CommandCode = PacketType.LeaderBoardData,
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_db?.GetTopPlayers(AppSettings.LeaderboardSize)))
            };
        }
        catch (Exception)
        {
            responsePacket = new NetworkPacket
            {
                CommandCode = PacketType.LeaderBoardData,
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        await SendResponse(client, responsePacket);
    }

    private static async Task DoRoomListForWathcers(NetworkPacket networkPacket, TcpClient client)
    {
        NetworkPacket responsePacket;
        try
        {
            if (networkPacket.CommandCode != PacketType.RoomsListForWatchersPacket) throw new NotImplementedException();
            var list = _rooms.Where(r => r.Started).Select(r => r.ToInfo()).ToList();
            responsePacket = new NetworkPacket
            {
                CommandCode = PacketType.RoomsListForWatchersPacket,
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(list))
            };
        }
        catch (Exception)
        {
            responsePacket = new NetworkPacket
            {
                CommandCode = PacketType.RoomsListForWatchersPacket,
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Failed,
            };
        }
        await SendResponse(client, responsePacket);
    }

    private static async Task DoFindGame(NetworkPacket networkPacket, TcpClient client)
    {
        try
        {
            if (networkPacket.CommandCode != PacketType.FindGame) throw new NotImplementedException();
            var ctx = GetContext(client);
            var data = JsonSerializer.Deserialize<FindGameData>(networkPacket.Data)
                       ?? throw new Exception("Invalid data");
            var user = ctx.User ?? _db?.GetUser(data.UserName) ?? throw new Exception("Not logged in");

            var player = new PlayerUser
            {
                UserId = user.Id,
                Name = user.Name,
                Raiting = user.Rating,
                Role = ClientType.Player,
                ClientConnection = ctx.Connection
            };
            ctx.Player = player;

            PlayerUser? opponent = null;
            ClientConnection? opponentConn = null;
            var queueList = _searchQueue.ToList();
            foreach (var item in queueList)
            {
                if (item.Conn.Tcp != client && !string.Equals(item.Player.Name, player.Name, StringComparison.OrdinalIgnoreCase))
                {
                    opponent = item.Player;
                    opponentConn = item.Conn;
                    break;
                }
            }

            if (opponent != null && opponentConn != null)
            {
                var newQueue = _searchQueue.Where(q => q.Conn.Tcp != client && q.Conn.Tcp != opponentConn.Tcp).ToList();
                _searchQueue.Clear();
                foreach (var q in newQueue) _searchQueue.Enqueue(q);

                var room = new RoomMatch(_nextRoomId++, opponent, player);
                room.StartMatch();
                _rooms.Add(room);

                opponent.Color = PlayerColor.White;
                player.Color = PlayerColor.Black;
                opponent.ClientConnection = opponentConn;
                player.ClientConnection = ctx.Connection;

                if (_clients.TryGetValue(opponentConn.Tcp, out var oppCtx))
                {
                    oppCtx.Player = opponent;
                    oppCtx.RoomId = room.Id;
                }
                ctx.RoomId = room.Id;

                await SendMatchStarted(client, room, player);
                await SendMatchStarted(opponentConn.Tcp, room, opponent);
                await SendBoardState(room);
            }
            else
            {
                _searchQueue.Enqueue((ctx.Connection, player));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static async Task DoCancelFindGame(TcpClient client)
    {
        var queueList = _searchQueue.Where(q => q.Conn.Tcp != client).ToList();
        _searchQueue.Clear();
        foreach (var q in queueList) _searchQueue.Enqueue(q);
        await Task.CompletedTask;
    }

    private static async Task SendMatchStarted(TcpClient client, RoomMatch room, PlayerUser player)
    {
        var data = new MatchStartedData
        {
            RoomId = room.Id,
            WhiteName = room.White?.Name ?? "",
            BlackName = room.Black?.Name ?? "",
            YourColor = player.Color,
            Role = ClientType.Player,
            InitialBoard = room.Board
        };
        await SendResponse(client, new NetworkPacket
        {
            CommandCode = PacketType.MatchStartedPacket,
            PacketTo = PacketWhere.ToClient,
            PResult = PacketResult.Success,
            Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data))
        });
    }

    private static async Task SendBoardState(RoomMatch room)
    {
        var packet = new NetworkPacket
        {
            CommandCode = PacketType.BoardStatePacket,
            PacketTo = PacketWhere.ToClient,
            PResult = PacketResult.Success,
            Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(room.Board))
        };
        await room.SendAllAsync(packet);
    }

    private static async Task DoMoveSync(NetworkPacket networkPacket, TcpClient client)
    {
        try
        {
            if (networkPacket.CommandCode != PacketType.MoveSyncPacket) throw new NotImplementedException();
            var data = JsonSerializer.Deserialize<MoveSyncData>(networkPacket.Data)
                       ?? throw new Exception("Invalid data");
            var room = _rooms.FirstOrDefault(r => r.Id == data.RoomId)
                       ?? throw new Exception("Room not found");
            var ctx = GetContext(client);
            var player = ctx.Player;
            if (player == null)
            {
                if (room.White?.ClientConnection.Tcp == client) player = room.White;
                else if (room.Black?.ClientConnection.Tcp == client) player = room.Black;
            }
            if (player == null) throw new Exception("Not a player");

            var color = room.GetColor(player) ?? throw new Exception("No color");
            bool ok = CheckersEngine.TryApplyMove(room.Board, data.Move, color);

            if (!ok)
            {
                await SendResponse(client, new NetworkPacket
                {
                    CommandCode = PacketType.MoveSyncPacket,
                    PacketTo = PacketWhere.ToClient,
                    PResult = PacketResult.Failed,
                    Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new MoveSyncData
                    {
                        RoomId = data.RoomId,
                        Move = data.Move,
                        Success = false,
                        Error = "Invalid move"
                    }))
                });
                return;
            }

            room.MoveHistory.Add($"{data.Move.FromRow},{data.Move.FromColumn}->{data.Move.ToRow},{data.Move.ToColumn}");
            await SendBoardState(room);

            if (room.Board.Result != GameState.InProgress)
                await FinishGame(room);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static async Task FinishGame(RoomMatch room)
    {
        if (room.White == null || room.Black == null) return;
        await FinishGameInternal(room, room.White, room.Black, false);
    }

    private static async Task ForfeitAndFinish(RoomMatch room, PlayerUser leaver)
    {
        if (room.Board.Result == GameState.InProgress)
            room.Board.Result = leaver.Color == PlayerColor.White ? GameState.BlackWin : GameState.WhiteWin;

        var whitePlayer = room.White ?? (leaver.Color == PlayerColor.White ? leaver : null);
        var blackPlayer = room.Black ?? (leaver.Color == PlayerColor.Black ? leaver : null);
        if (whitePlayer == null || blackPlayer == null) return;

        await FinishGameInternal(room, whitePlayer, blackPlayer, true);
    }

    private static async Task FinishGameInternal(RoomMatch room, PlayerUser whitePlayer, PlayerUser blackPlayer, bool byForfeit)
    {
        var whiteUser = _db?.GetUser(whitePlayer.UserId);
        var blackUser = _db?.GetUser(blackPlayer.UserId);
        if (whiteUser == null || blackUser == null) return;

        switch (room.Board.Result)
        {
            case GameState.WhiteWin:
                whiteUser.Rating += 10;
                blackUser.Rating = Math.Max(0, blackUser.Rating - 10);
                break;
            case GameState.BlackWin:
                blackUser.Rating += 10;
                whiteUser.Rating = Math.Max(0, whiteUser.Rating - 10);
                break;
        }

        _db?.UpdateUser(whiteUser);
        _db?.UpdateUser(blackUser);
        _db?.AddGame(new GameHistory
        {
            WhiteId = whiteUser.Id,
            BlackId = blackUser.Id,
            WinnerId = room.Board.Result switch
            {
                GameState.WhiteWin => whiteUser.Id,
                GameState.BlackWin => blackUser.Id,
                _ => null
            },
            Moves = string.Join(";", room.MoveHistory),
            Date = DateTime.UtcNow
        });

        var endData = new GameEndData
        {
            RoomId = room.Id,
            Result = room.Board.Result,
            ByForfeit = byForfeit,
            WhiteUser = whiteUser,
            BlackUser = blackUser
        };
        var packet = new NetworkPacket
        {
            CommandCode = PacketType.GameEndPacket,
            PacketTo = PacketWhere.ToClient,
            PResult = PacketResult.Success,
            Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(endData))
        };
        await room.SendAllAsync(packet);
        _rooms.Remove(room);
    }

    private static async Task DoManageConnection(NetworkPacket networkPacket, TcpClient client)
    {
        try
        {
            if (networkPacket.CommandCode != PacketType.ManageConnectionPacket) throw new NotImplementedException();
            var data = JsonSerializer.Deserialize<ManageConnectionData>(networkPacket.Data)
                       ?? throw new Exception("Invalid data");
            var ctx = GetContext(client);

            if (data.ConnectionType == ConnectionType.Exit)
            {
                await HandleRoomLeaveAsync(client, ctx);
                return;
            }

            var watchRoom = _rooms.FirstOrDefault(r => r.Id == data.RoomId && r.Started)
                            ?? throw new Exception("Room not found");
            var watcher = new Watcher { clientConnection = ctx.Connection };
            watchRoom.AddWatcher(watcher);
            ctx.RoomId = watchRoom.Id;

            var matchData = new MatchStartedData
            {
                RoomId = watchRoom.Id,
                WhiteName = watchRoom.White?.Name ?? "",
                BlackName = watchRoom.Black?.Name ?? "",
                YourColor = null,
                Role = ClientType.Watcher,
                InitialBoard = watchRoom.Board
            };
            await SendResponse(client, new NetworkPacket
            {
                CommandCode = PacketType.MatchStartedPacket,
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(matchData))
            });

            await SendResponse(client, new NetworkPacket
            {
                CommandCode = PacketType.BoardStatePacket,
                PacketTo = PacketWhere.ToClient,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(watchRoom.Board))
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static void Stop()
    {
        _running = false;
        try { _serverListener?.Stop(); } catch { }
    }
}
