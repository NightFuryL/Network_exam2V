using System.Net.Sockets;
using System.Text;
namespace GameCore;
public class ClientConnection
{
    public TcpClient Tcp { get; }
    public ClientConnection(TcpClient tcp) => Tcp = tcp;
    public void Send(INetworkPacket packet)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(packet.Pack() + "\n");
            Tcp.GetStream().Write(data, 0, data.Length);
        }
        catch { }
    }
}