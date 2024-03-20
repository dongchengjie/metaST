using System.Net;
using System.Net.Sockets;

namespace Util;

public class PortManager : IDisposable
{
    private static readonly int startingPort = 50001;
    private static readonly int endingPort = 60000;
    private static int currentPort = startingPort;
    private readonly List<Socket> sockets = [];
    private readonly List<int> ports = [];
    private PortManager() { }
    public static PortManager Claim(int portNum)
    {
        PortManager manager = new();
        int acquiredPorts = 0;
        while (acquiredPorts < portNum)
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
                acquiredPorts++;
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

    private static string PortRanges(List<int> ports)
    {
        List<string> ranges = [];
        int start = ports[0];
        int end = ports[0];
        for (int i = 1; i < ports.Count; i++)
        {
            if (ports[i] == end + 1)
            {
                end = ports[i];
            }
            else
            {
                ranges.Add(start == end ? start.ToString() : $"{start}-{end}");
                start = end = ports[i];
            }
        }
        ranges.Add(start == end ? start.ToString() : $"{start}-{end}");
        return string.Join(",", ranges);
    }
}