using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameCore;

public class ClientConnection
{
    public TcpClient Tcp { get; }

    public ClientConnection(TcpClient tcp) => Tcp = tcp;

    public async Task<byte[]> ManagePacketClientAsync(byte[] sendBytes)
    {
        try
        {
            NetworkStream stream = Tcp.GetStream();

            // Отправка данных
            await stream.WriteAsync(sendBytes, 0, sendBytes.Length);

            // Чтение ответа
            byte[] buffer = new byte[4096];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0)
            {
                throw new Exception("Соединение закрыто удаленной стороной.");
            }

            byte[] response = new byte[bytesRead];
            Array.Copy(buffer, response, bytesRead);
            return response;
        }
        catch (Exception ex)
        {
            // Логируем ошибку или передаем наверх
            Console.WriteLine($"Ошибка сети: {ex.Message}");
            throw;
        }
    }
}
