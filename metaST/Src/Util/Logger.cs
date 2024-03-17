using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Timer = System.Timers.Timer;

namespace Util;
public class Logger
{
    // 默认日志级别
    public static LogLevel LogLevel { get; set; } = LogLevel.info;
    // 日志刷新间隔
    public static double RefreshInterval { get; set; } = 1000;
    // 控制台原始颜色
    public static readonly ConsoleColor primitiveColor = Console.ForegroundColor;
    protected static readonly Timer timer;
    // 日志队列
    protected static readonly ConcurrentQueue<Log> queue = new();
    // 日志输出文件
    protected static readonly string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", DateTimeOffset.Now.ToUnixTimeSeconds() + ".log");
    static Logger()
    {
        // 设置控制台编码
        Console.OutputEncoding = Encoding.UTF8;
        // 启动定时器
        timer = new Timer { Interval = RefreshInterval };
        timer.Elapsed += (sender, e) => Flush();
        timer.Start();
    }

    public static void Terminate()
    {
        timer.Stop();
        Flush();
    }

    private static void Flush()
    {
        while (queue.TryDequeue(out var log))
        {
            PrintAndWrite(log);
        }
    }
    private static void PrintAndWrite(Log log)
    {
        lock (queue)
        {
            Console.ForegroundColor = log.Color;
            Console.WriteLine(log);
            Console.ForegroundColor = primitiveColor;
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(log + Environment.NewLine));
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log"));
            using FileStream fileStream = new(logFile, FileMode.Append);
            stream.CopyTo(fileStream);
        }
    }

    public static void Log(LogLevel level, object message, ConsoleColor color)
    {
        if (level >= LogLevel)
        {
            Log log = new(level, message, color);
            // trace级别直接输出
            if (LogLevel == LogLevel.trace)
            {
                PrintAndWrite(log);
            }
            else
            {
                queue.Enqueue(log);
            }
        }
    }

    public static void Trace(object msg) => Log(LogLevel.trace, msg, ConsoleColor.Gray);
    public static void Debug(object msg) => Log(LogLevel.debug, msg, ConsoleColor.Blue);
    public static void Info(object msg) => Log(LogLevel.info, msg, ConsoleColor.Green);
    public static void Warn(object msg) => Log(LogLevel.warn, msg, ConsoleColor.DarkYellow);
    public static void Error(object msg) => Log(LogLevel.error, msg, ConsoleColor.Red);
    public static void Fatal(object msg) => Log(LogLevel.fatal, msg, ConsoleColor.DarkRed);
}

public enum LogLevel
{
    trace = 1,
    debug = 2,
    info = 4,
    warn = 6,
    error = 8,
    fatal = 16
}

public class Log
{
    public LogLevel Level { get; set; }
    public object Message { get; set; }
    public ConsoleColor Color { get; set; }
    public DateTime DateTime { get; set; }
    public StackTrace StackTrace { get; set; }
    public string Location { get; set; }

    public Log(LogLevel logLevel, object message, ConsoleColor color)
    {
        Level = logLevel;
        Message = message;
        Color = color;
        StackTrace = new StackTrace(true);
        StackFrame? stackFrame = StackTrace.GetFrames().Skip(3).FirstOrDefault();
        Location =
            stackFrame?.GetMethod()?.DeclaringType?.FullName
            + " # " + stackFrame?.GetMethod()
            + " at line " + stackFrame?.GetFileLineNumber() + ", col " + stackFrame?.GetFileColumnNumber();
        DateTime = DateTime.Now;
    }

    public override string ToString()
    {
        string format = Logger.LogLevel switch
        {
            LogLevel.trace => "[{0:yyyy-MM-dd HH:mm:ss}] [{1,-5}] {2} at {4}",
            LogLevel.debug => "[{0:yyyy-MM-dd HH:mm:ss}] [{1,-5}] {2} at {3}",
            _ => "[{0:yyyy-MM-dd HH:mm:ss}] [{1,-5}] {2}",
        };
        return string.Format(format, [
            DateTime,Level,Message,Location,
            "\nStackTrace: \n  "  + string.Join("  ", StackTrace.GetFrames().Select((f) => f.ToString()))
        ]);
    }
}