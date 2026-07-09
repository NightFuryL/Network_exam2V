using DatabaseLibrary.Core;
using GameCore;

namespace ServerCTask;

internal class Program
{
    static void Main()
    {
        Console.WriteLine($"Сервер {AppSettings.Host}:{AppSettings.Port}");
        GameServer.Start(new DataManager());
        Console.ReadLine();
        GameServer.Stop();
    }
}
