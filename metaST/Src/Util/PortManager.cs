using System.Net;
using System.Net.Sockets;

namespace Util;

public class PortManager : IDisposable
{
    private static readonly int startingPort = 50000;
    private static readonly int endingPort = 65000;
    private static int currentPort = startingPort + new Random().Next(endingPort - startingPort);
    private readonly List<Socket> sockets = [];
    private readonly List<int> ports = [];
    private PortManager() { }
    public static PortManager Claim(int portNum)
    {
        PortManager manager = new();
        int acquired = 0;
        while (acquired < portNum)
        {
            Socket? socket = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localEP = new(IPAddress.Loopback, currentPort);
                socket.Bind(localEP);
                manager.sockets.Add(socket);
                IPEndPoint? endPoint = (IPEndPoint?)socket.LocalEndPoint;
                manager.ports.Add(endPoint != null ? endPoint.Port : -1);
                acquired++;
            }
            catch
            {
                CloseSocket(socket);
                Thread.Sleep(100);
            }
            finally
            {
                currentPort = currentPort + 1 <= endingPort ? currentPort + 1 : startingPort;
            }
        }
        return manager;
    }

    public int Get(int index)
    {
        return ports[index];
    }

    public int Use(int index)
    {
        CloseSocket(sockets[index]);
        return ports[index];
    }

    public void Dispose()
    {
        sockets.ForEach(CloseSocket);
        GC.SuppressFinalize(this);
    }

    private static void CloseSocket(Socket? socket)
    {
        if (socket != null) using (socket) { }
    }
}