using DatabaseLibrary.Core;
using GameCore;
using System.Net.Sockets;

namespace ClientWFTask;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new RegisterForm());
    }
}
