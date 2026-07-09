using System.Net.Sockets;
using System.Text.Json;
using GameCore;

namespace ClientWFTask.Services;

public sealed class GameConnection
{
    private static GameConnection? _instance;
    public static GameConnection Instance => _instance ??= new GameConnection();

    private TcpClient? _client;
    private ClientConnection? _connection;
    private CancellationTokenSource? _readCts;
    private Task? _readTask;

    public event Action<NetworkPacket>? PacketReceived;

    private GameConnection() { }

    public void EnsureConnected()
    {
        if (_client?.Connected == true) return;
        _client = new TcpClient();
        _client.Connect(AppSettings.Host, AppSettings.Port);
        _connection = new ClientConnection(_client);
        _readCts = new CancellationTokenSource();
        _readTask = Task.Run(() => ReadLoop(_readCts.Token));
    }

    public async Task<NetworkPacket> SendAndWaitAsync(NetworkPacket packet, PacketType expectedType)
    {
        EnsureConnected();
        var tcs = new TaskCompletionSource<NetworkPacket>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Handler(NetworkPacket p)
        {
            if (p.CommandCode == expectedType)
            {
                PacketReceived -= Handler;
                tcs.TrySetResult(p);
            }
        }
        PacketReceived += Handler;
        await _connection!.SendAsync(packet);
        return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
    }

    public async Task SendAsync(NetworkPacket packet)
    {
        EnsureConnected();
        await _connection!.SendAsync(packet);
    }

    private async Task ReadLoop(CancellationToken token)
    {
        try
        {
            var stream = _client!.GetStream();
            byte[] header = new byte[7];
            while (!token.IsCancellationRequested)
            {
                await stream.ReadExactlyAsync(header, token);
                int dataLength = BitConverter.ToInt32(header, 3);
                byte[] full = new byte[7 + dataLength];
                Array.Copy(header, 0, full, 0, 7);
                if (dataLength > 0)
                    await stream.ReadExactlyAsync(full.AsMemory(7, dataLength), token);
                var packet = NetworkPacket.Deserialize(full);
                PacketReceived?.Invoke(packet);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public void Disconnect()
    {
        _readCts?.Cancel();
        _client?.Close();
        _client = null;
        _connection = null;
    }
}
