using GameCore;
using System.Net.Sockets;
using System.Text;

namespace ClientWFTask.Services;

public class GameConnection
{
    public static GameConnection Instance { get; } = new();

    private TcpClient? _tcp;
    private readonly StringBuilder _buffer = new();

    public event Action<INetworkPacket>? PacketReceived;

    public bool IsConnected => _tcp?.Connected == true;

    public void Connect()
    {
        if (IsConnected) return;
        _tcp = new TcpClient();
        _tcp.Connect(AppSettings.Host, AppSettings.Port);
        Task.Run(ReadLoop);
    }

    public void Send(INetworkPacket packet)
    {
        if (!IsConnected) Connect();
        byte[] data = Encoding.UTF8.GetBytes(packet.Pack() + "\n");
        _tcp!.GetStream().Write(data, 0, data.Length);
    }

    private void ReadLoop()
    {
        if (_tcp == null) return;
        byte[] buf = new byte[4096];
        while (_tcp.Connected)
        {
            try
            {
                int n = _tcp.GetStream().Read(buf, 0, buf.Length);
                if (n <= 0) break;
                _buffer.Append(Encoding.UTF8.GetString(buf, 0, n));
                while (ReadLine(out string? line))
                    if (!string.IsNullOrWhiteSpace(line))
                        PacketReceived?.Invoke(PacketFactory.Parse(line));
            }
            catch { break; }
        }
    }

    private bool ReadLine(out string? line)
    {
        line = null;
        string all = _buffer.ToString();
        int i = all.IndexOf('\n');
        if (i < 0) return false;
        line = all[..i].Trim();
        _buffer.Clear();
        _buffer.Append(all[(i + 1)..]);
        return true;
    }
}
