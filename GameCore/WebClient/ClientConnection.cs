using System.Net.Sockets;

namespace GameCore;

public class ClientConnection
{
    public TcpClient Tcp { get; }
    private readonly NetworkStream _stream;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public ClientConnection(TcpClient tcp)
    {
        Tcp = tcp;
        _stream = tcp.GetStream();
    }

    public async Task SendAsync(NetworkPacket packet)
    {
        byte[] bytes = packet.Serialize();
        await _sendLock.WaitAsync();
        try
        {
            await _stream.WriteAsync(bytes);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task<byte[]> ManagePacketClientAsync(byte[] sendBytes)
    {
        try
        {
            await _stream.WriteAsync(sendBytes, 0, sendBytes.Length);

            byte[] headerBuffer = new byte[7];
            await _stream.ReadExactlyAsync(headerBuffer, 0, headerBuffer.Length);
            int dataLength = BitConverter.ToInt32(headerBuffer, 3);
            byte[] fullPacketBytes = new byte[7 + dataLength];
            Array.Copy(headerBuffer, 0, fullPacketBytes, 0, 7);
            if (dataLength > 0)
                await _stream.ReadExactlyAsync(fullPacketBytes, 7, dataLength);
            return fullPacketBytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сети: {ex.Message}");
            throw;
        }
    }
}
