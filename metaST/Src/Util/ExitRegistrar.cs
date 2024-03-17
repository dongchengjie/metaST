using System.Collections.Concurrent;
using System.Runtime.InteropServices;

#pragma warning disable SYSLIB1054

namespace Util;
public class ExitRegistrar
{
    private static readonly ConcurrentQueue<Action<EventType>> actions = new();

    // 注册退出事件
    public static void RegisterAction(Action<EventType> action) => actions.Enqueue(action);

    private delegate bool HandlerRoutine(EventType eventType);
    [DllImport("Kernel32")]

    private static extern bool SetConsoleCtrlHandler(HandlerRoutine handlerRoutine, bool add);
    static ExitRegistrar()
    {
        var handler = new HandlerRoutine((EventType type) =>
        {
            switch (type)
            {
                case EventType.CTRL_C_EVENT:
                case EventType.CTRL_BREAK_EVENT:
                case EventType.CLOSE_EVENT:
                case EventType.LOGOFF_EVENT:
                case EventType.SHUTDOWN_EVENT:
                    {
                        try
                        {
                            Console.WriteLine("Waiting for program to exit...");
                            while (actions.TryDequeue(out var action))
                            {
                                action.Invoke(type);
                            }
                            Console.WriteLine("Program exited");
                            return true;
                        }
                        finally
                        {
                            Environment.Exit(-1);
                        }
                    }
                default: return false;
            }
        });

        // 将委托实例固定在内存中，以确保不会被垃圾回收
        IntPtr ptr = GCHandle.ToIntPtr(GCHandle.Alloc(handler));
        SetConsoleCtrlHandler(handler, true);
    }
}
public enum EventType
{
    CTRL_C_EVENT = 0,       // 按下Ctrl + C
    CTRL_BREAK_EVENT = 1,   // 按下Ctrl + Break
    CLOSE_EVENT = 2,        // 关闭控制台程序
    LOGOFF_EVENT = 5,       // 用户退出
    SHUTDOWN_EVENT = 6      // 系统被关闭
}

